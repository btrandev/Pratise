using Common.Middleware.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Common.Middleware.Performance.Caching;

/// <summary>
/// Middleware for implementing a custom response caching strategy
/// </summary>
public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly CachingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cache">The memory cache</param>
    /// <param name="options">The options for the middleware</param>
    public CachingMiddleware(
        RequestDelegate next, 
        IMemoryCache cache,
        IOptions<CachingOptions> options)
    {
        _next = next;
        _cache = cache;
        _options = options.Value;
    }

    /// <summary>
    /// Processes an HTTP request by applying response caching
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Only cache GET or HEAD requests
        if (!HttpMethods.IsGet(context.Request.Method) && 
            !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Build a cache key from the request path and query
        var cacheKey = $"{context.Request.Path}{context.Request.QueryString}";
        
        // Try to get the cached response
        if (_cache.TryGetValue(cacheKey, out CachedResponse cachedResponse))
        {
            // Apply cached response
            context.Response.StatusCode = cachedResponse.StatusCode;
            foreach (var header in cachedResponse.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }
            
            await context.Response.Body.WriteAsync(cachedResponse.Body);
            return;
        }

        // Capture the response to cache it
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Call the next middleware
        await _next(context);

        // Cache the response if it's cacheable
        if (IsCacheable(context))
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBytes = responseBody.ToArray();

            // Store in cache
            var response = new CachedResponse
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBytes,
                Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value)
            };
            
            // Get cache duration from the cache profile or use default
            var duration = GetCacheDuration(context);
            _cache.Set(cacheKey, response, TimeSpan.FromSeconds(duration));
            
            // Copy the response to the original stream
            await originalBodyStream.WriteAsync(responseBytes);
        }
        else
        {
            // Not cacheable, just copy the response
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
    
    private bool IsCacheable(HttpContext context)
    {
        // Check if the response is cacheable
        // Status code should be 200 (OK)
        if (context.Response.StatusCode != StatusCodes.Status200OK)
        {
            return false;
        }
        
        // Check for Cache-Control headers
        if (context.Response.Headers.CacheControl.ToString().Contains("no-store") ||
            context.Response.Headers.CacheControl.ToString().Contains("no-cache"))
        {
            return false;
        }
        
        return true;
    }
    
    private int GetCacheDuration(HttpContext context)
    {
        // Check if there's a cache duration header
        if (context.Response.Headers.CacheControl.ToString()
            .Contains("max-age=", StringComparison.OrdinalIgnoreCase))
        {
            var cacheControl = context.Response.Headers.CacheControl.ToString();
            var maxAgeIndex = cacheControl.IndexOf("max-age=", StringComparison.OrdinalIgnoreCase);
            var maxAgeValue = cacheControl[(maxAgeIndex + 8)..].Split(',')[0];
            
            if (int.TryParse(maxAgeValue, out var seconds))
            {
                return seconds;
            }
        }
        
        // Use default profile
        return _options.CacheProfiles["Default"].Duration;
    }
}

/// <summary>
/// Represents a cached HTTP response
/// </summary>
internal class CachedResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Gets or sets the response body
    /// </summary>
    public byte[] Body { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Gets or sets the response headers
    /// </summary>
    public Dictionary<string, Microsoft.Extensions.Primitives.StringValues> Headers { get; set; } = 
        new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
}
