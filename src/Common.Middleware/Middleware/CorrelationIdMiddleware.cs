using Microsoft.AspNetCore.Http;
using Serilog.Context;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if request has correlation id
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            // If not, use the trace identifier
            correlationId = context.TraceIdentifier;
        }
        
        // Add or update the correlation ID header in the response
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });
        
        // Add to log context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}