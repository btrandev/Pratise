using Common.Middleware.Authorization;
using Common.Middleware.Options;
using Common.Middleware.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Common.Middleware.Middleware;

/// <summary>
/// Middleware for handling exceptions globally
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExceptionHandlingOptions _options;
    private readonly ILogger<ExceptionHandlingMiddleware>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="options">Options for configuring the middleware</param>
    /// <param name="logger">Logger for recording exceptions</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ExceptionHandlingOptions options,
        ILogger<ExceptionHandlingMiddleware>? logger = null)
    {
        _next = next;
        _options = options ?? new ExceptionHandlingOptions();
        _logger = logger;
    }

    /// <summary>
    /// Processes an HTTP request and catches any unhandled exceptions
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        // Get the correlation ID from the HttpContext
        string correlationId = GetCorrelationId(context);

        if (_options.LogExceptions)
        {
            _logger?.LogError(exception, "An unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);
        }

        // Get appropriate error message for problem details
        var errorMessage = _options.IncludeExceptionDetails ? exception.Message : _options.DefaultErrorMessage;

        var problemDetails = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = GetTitle(exception),
            Detail = errorMessage,
            Instance = context.Request.Path
        };

        // Add exception details if enabled
        if (_options.IncludeExceptionDetails)
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;

            if (exception.StackTrace != null)
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }
        }

        // For unexpected exceptions, only include the problem details - omit errors array
        object result = new
        {
            IsSuccess = false,
            Errors = new object[] { problemDetails },
            CorrelationId = correlationId
        };

        // Serialize the Result with ProblemDetails
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, options));
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            AuthorizationException => (int)HttpStatusCode.Forbidden,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            AuthorizationException => "Authorization Failed",
            _ => "An error occurred"
        };
    }

    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            AuthorizationException => "AUTH_ERROR",
            _ => "INTERNAL_ERROR"
        };
    }

    /// <summary>
    /// Gets the correlation ID from the HTTP context
    /// </summary>
    private static string GetCorrelationId(HttpContext context)
    {
        const string correlationIdHeader = "X-Correlation-ID";

        // First check if the correlation ID is in the request headers
        if (context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        // If not in headers, check response (might have been set by middleware)
        if (context.Response.Headers.TryGetValue(correlationIdHeader, out correlationId))
        {
            return correlationId.ToString();
        }

        // If all else fails, use the trace identifier as a fallback
        return context.TraceIdentifier;
    }
}
