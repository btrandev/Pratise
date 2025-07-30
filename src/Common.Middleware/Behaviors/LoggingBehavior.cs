using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using Common.Middleware.Helpers;
using Common.Middleware.Options;
using Microsoft.Extensions.Options;

namespace Common.Middleware.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs requests and their execution time
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly LoggingOptions _loggingOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="loggingOptions">The logging options</param>
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IOptions<LoggingOptions> loggingOptions)
    {
        _logger = logger;
        _loggingOptions = loggingOptions?.Value ?? new LoggingOptions();
    }

    /// <summary>
    /// Handles the request by logging it before and after execution
    /// </summary>
    /// <param name="request">The request to log</param>
    /// <param name="next">The delegate to the next handler</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The response from the next handler</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        if (_loggingOptions.LogPayloads)
        {
            // Optimize: Only use expensive sanitization if the request has sensitive properties
            if (SensitiveDataHelper.HasSensitiveProperties(request))
            {
                var sanitizedPayload = SensitiveDataHelper.SanitizePayload(request);
                
                // Truncate if payload is too large
                if (sanitizedPayload.Length > _loggingOptions.MaxPayloadSize)
                {
                    sanitizedPayload = sanitizedPayload.Substring(0, _loggingOptions.MaxPayloadSize) + "... [truncated]";
                }
                
                _logger.LogInformation("Starting request {RequestName}. Payload: {Payload}", 
                    requestName, sanitizedPayload);
            }
            else
            {
                // If no sensitive properties, we can just log the request as is with basic serialization
                var simplePayload = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                // Truncate if needed
                if (simplePayload.Length > _loggingOptions.MaxPayloadSize)
                {
                    simplePayload = simplePayload.Substring(0, _loggingOptions.MaxPayloadSize) + "... [truncated]";
                }
                
                _logger.LogInformation("Starting request {RequestName}. Payload: {Payload}", 
                    requestName, simplePayload);
            }
        }
        else
        {
            _logger.LogInformation("Starting request {RequestName}", requestName);
        }
        
        try 
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation("Completed request {RequestName} in {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestName}: {ErrorMessage}", 
                requestName, ex.Message);
            throw;
        }
    }
}
