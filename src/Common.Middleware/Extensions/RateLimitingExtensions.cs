using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring rate limiting middleware
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting services to the service collection with customizable options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional delegate to configure rate limiting options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        Action<RateLimitingOptions>? configureOptions = null)
    {
        // Configure options if provided
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        services.AddRateLimiter(options =>
        {
            var rateLimitingOptions = services.BuildServiceProvider()
                .GetService<IOptions<RateLimitingOptions>>()?.Value
                ?? new RateLimitingOptions();
            
            // Add a fixed window limiter named "fixed"
            if (rateLimitingOptions.EnableFixedWindowLimiter)
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = rateLimitingOptions.FixedWindowPermitLimit;
                    opt.Window = TimeSpan.FromSeconds(rateLimitingOptions.FixedWindowDurationSeconds);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = rateLimitingOptions.FixedWindowQueueLimit;
                });
            }
            
            // Add a sliding window limiter named "sliding"
            if (rateLimitingOptions.EnableSlidingWindowLimiter)
            {
                options.AddSlidingWindowLimiter("sliding", opt =>
                {
                    opt.PermitLimit = rateLimitingOptions.SlidingWindowPermitLimit;
                    opt.Window = TimeSpan.FromSeconds(rateLimitingOptions.SlidingWindowDurationSeconds);
                    opt.SegmentsPerWindow = rateLimitingOptions.SlidingWindowSegments;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = rateLimitingOptions.SlidingWindowQueueLimit;
                });
            }
            
            // Add a token bucket limiter named "token"
            if (rateLimitingOptions.EnableTokenBucketLimiter)
            {
                options.AddTokenBucketLimiter("token", opt =>
                {
                    opt.TokenLimit = rateLimitingOptions.TokenBucketLimit;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = rateLimitingOptions.TokenBucketQueueLimit;
                    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitingOptions.TokenBucketReplenishmentPeriodSeconds);
                    opt.TokensPerPeriod = rateLimitingOptions.TokenBucketTokensPerPeriod;
                    opt.AutoReplenishment = true;
                });
            }
            
            // Add an endpoint-specific limiter (1 request per second)
            options.AddFixedWindowLimiter("endpoint-strict", opt =>
            {
                opt.PermitLimit = 1;
                opt.Window = TimeSpan.FromSeconds(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0; // No queue, reject immediately when rate limit is exceeded
            });
            
            // Add an IP-based rate limiter (1 request per second per IP)
            options.AddPolicy("ip-rate-limit-per-second", context =>
            {
                // Get client IP address
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = 0, // No queue, reject immediately when rate limit is exceeded
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
            
            // Add an IP-based rate limiter (5 requests per minute per IP)
            options.AddPolicy("ip-rate-limit-per-minute", context =>
            {
                // Get client IP address
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
            
            // Add an IP-based rate limiter (30 requests per hour per IP)
            options.AddPolicy("ip-rate-limit-per-hour", context =>
            {
                // Get client IP address
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
            });
            
            // Configure the global rate limiting policy
            if (rateLimitingOptions.EnableGlobalLimiter)
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    return RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingOptions.GlobalPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingOptions.GlobalWindowDurationSeconds),
                        QueueLimit = rateLimitingOptions.GlobalQueueLimit
                    });
                });
            }
            
            // Configure what happens when a request is rejected
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "text/plain";
                await context.HttpContext.Response.WriteAsync(rateLimitingOptions.RejectionMessage, token);
            };
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds the rate limiting middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
    
    /// <summary>
    /// Applies strict rate limiting (1 request per second) to specific endpoints, partitioned by client IP
    /// </summary>
    /// <param name="builder">The endpoint route builder</param>
    /// <returns>The endpoint route builder for chaining</returns>
    public static RouteHandlerBuilder RequireRateLimitingPerSecond(this RouteHandlerBuilder builder)
    {
        // Use the named IP-based policy we defined
        return builder.RequireRateLimiting("ip-rate-limit-per-second");
    }
    
    /// <summary>
    /// Applies custom rate limiting to specific endpoints (5 requests per minute), partitioned by client IP
    /// </summary>
    /// <param name="builder">The endpoint route builder</param>
    /// <returns>The endpoint route builder for chaining</returns>
    public static RouteHandlerBuilder RequireRateLimitingByIpPerMinute(this RouteHandlerBuilder builder)
    {
        // Add a policy for 5 requests per minute per IP
        return builder.RequireRateLimiting("ip-rate-limit-per-minute");
    }
    
    /// <summary>
    /// Applies custom rate limiting to specific endpoints (30 requests per hour), partitioned by client IP
    /// </summary>
    /// <param name="builder">The endpoint route builder</param>
    /// <returns>The endpoint route builder for chaining</returns>
    public static RouteHandlerBuilder RequireRateLimitingByIpPerHour(this RouteHandlerBuilder builder)
    {
        // Add a policy for 30 requests per hour per IP
        return builder.RequireRateLimiting("ip-rate-limit-per-hour");
    }
}
