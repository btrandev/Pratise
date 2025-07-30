using Common.Middleware.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Common.Middleware.Logging;

/// <summary>
/// Middleware for logging HTTP requests with their duration and status code
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options for the middleware</param>
    public RequestLoggingMiddleware(
        RequestDelegate next, 
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Processes an HTTP request by logging its details before and after execution
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;
        var requestId = context.TraceIdentifier;
        
        _logger.LogInformation(
            "Request started {RequestMethod} {RequestPath} {RequestId}", 
            requestMethod, 
            requestPath, 
            requestId);
            
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            stopwatch.Stop();
            
            var elapsed = stopwatch.ElapsedMilliseconds;
            var isSlowRequest = elapsed >= _options.SlowRequestThresholdMs;
            
            if (_options.LogSuccessfulRequests || isSlowRequest)
            {
                var logLevel = isSlowRequest ? LogLevel.Warning : LogLevel.Information;
                
                _logger.Log(
                    logLevel,
                    "Request completed {RequestMethod} {RequestPath} {RequestId} - {StatusCode} in {ElapsedMilliseconds}ms",
                    requestMethod,
                    requestPath,
                    requestId,
                    context.Response.StatusCode,
                    elapsed);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Request failed {RequestMethod} {RequestPath} {RequestId} in {ElapsedMilliseconds}ms",
                requestMethod,
                requestPath,
                requestId,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}
