using Common.Middleware.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading;

namespace Common.Middleware.RateLimiting;

/// <summary>
/// Custom middleware for implementing IP-based rate limiting
/// </summary>
public class IpRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;
    private readonly IMemoryCache _cache;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = 
        new ConcurrentDictionary<string, SemaphoreSlim>();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IpRateLimitingMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="options">The options for the middleware</param>
    /// <param name="cache">The memory cache</param>
    public IpRateLimitingMiddleware(
        RequestDelegate next, 
        IOptions<RateLimitingOptions> options,
        IMemoryCache cache)
    {
        _next = next;
        _options = options.Value;
        _cache = cache;
    }

    /// <summary>
    /// Processes an HTTP request by applying rate limiting
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Get client IP address
        var clientIp = GetClientIpAddress(context);
        
        // Create a rate limiting key based on client IP
        var key = $"ratelimit_{clientIp}";
        
        // Ensure we have a semaphore for this IP
        var semaphore = _semaphores.GetOrAdd(key, _ => 
            new SemaphoreSlim(_options.FixedWindowPermitLimit, _options.FixedWindowPermitLimit));
        
        // Try to get access
        bool access = false;
        
        try
        {
            access = await semaphore.WaitAsync(0); // Don't block, just check if we can get access
            
            if (!access)
            {
                // Access denied, too many requests
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = _options.FixedWindowDurationSeconds.ToString();
                await context.Response.WriteAsync(_options.RejectionMessage);
                return;
            }
            
            // Check if this IP has exceeded the request count
            if (!_cache.TryGetValue(key, out int requestCount))
            {
                // First request, add to cache
                requestCount = 1;
                
                // Cache for the window duration
                _cache.Set(key, requestCount, TimeSpan.FromSeconds(_options.FixedWindowDurationSeconds));
            }
            else
            {
                // Increment request count
                requestCount++;
                _cache.Set(key, requestCount, TimeSpan.FromSeconds(_options.FixedWindowDurationSeconds));
                
                // Check if exceeding limit
                if (requestCount > _options.FixedWindowPermitLimit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = _options.FixedWindowDurationSeconds.ToString();
                    await context.Response.WriteAsync(_options.RejectionMessage);
                    return;
                }
            }
            
            // Set headers for rate limit info
            context.Response.Headers["X-RateLimit-Limit"] = _options.FixedWindowPermitLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = 
                Math.Max(0, _options.FixedWindowPermitLimit - requestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = 
                DateTimeOffset.UtcNow.AddSeconds(_options.FixedWindowDurationSeconds).ToUnixTimeSeconds().ToString();
            
            await _next(context);
        }
        finally
        {
            // Release the semaphore if we got access
            if (access)
            {
                semaphore.Release();
            }
        }
    }
    
    private string GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from headers first (for proxied requests)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
