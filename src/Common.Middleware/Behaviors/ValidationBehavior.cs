using Common.Middleware.Results;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Common.Middleware.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class
    /// </summary>
    /// <param name="validators">The validators for the request</param>
    /// <param name="logger">Optional logger</param>
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>>? logger = null)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Handles the request by validating it before passing it to the next handler
    /// </summary>
    /// <param name="request">The request to validate</param>
    /// <param name="next">The delegate to the next handler</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The response from the next handler</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Running validation behavior for {RequestType}", typeof(TRequest).Name);

        // Check if we have any validators
        if (!_validators.Any())
        {
            _logger?.LogDebug("No validators found for {RequestType}", typeof(TRequest).Name);
            return await next();
        }

        _logger?.LogDebug("Found {ValidatorCount} validators for {RequestType}", _validators.Count(), typeof(TRequest).Name);

        // Execute each validator with the request directly, not a ValidationContext
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        var failures = validationResults
            .Where(r => r != null)
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            _logger?.LogInformation("Validation failed for {RequestType} with {FailureCount} errors", typeof(TRequest).Name, failures.Count);
            var validationErrors = failures.Select(error => new Error(
                        string.IsNullOrEmpty(error.PropertyName)
                            ? "VALIDATION_ERROR"
                            : $"VALIDATION_ERROR_{error.PropertyName.ToUpperInvariant()}",
                        error.ErrorMessage))
                    .ToArray();
            var dataType = typeof(TResponse).GetGenericArguments()[0];
            var resultType = typeof(Result<>).MakeGenericType(dataType);
            var failureMethod = resultType.GetMethod("Failure", new[] { typeof(IEnumerable<Error>) });
            if (failureMethod != null)
            {
                var result = failureMethod.Invoke(null, [validationErrors]);
                return (TResponse)result!;
            }
        }

        return await next();
    }
}
