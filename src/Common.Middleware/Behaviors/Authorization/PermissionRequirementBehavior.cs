using MediatR;
using Microsoft.Extensions.Logging;
using Common.Middleware.Authorization;
using Common.Middleware.Results;

namespace Common.Middleware.Authorization;

/// <summary>
/// MediatR pipeline behavior for enforcing permission requirements
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PermissionRequirementBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PermissionRequirementBehavior<TRequest, TResponse>> _logger;

    public PermissionRequirementBehavior(
        ICurrentUser currentUser,
        ILogger<PermissionRequirementBehavior<TRequest, TResponse>> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        // Check if the request requires authorization
        if (request is IRequireAuthorization requireAuth)
        {
            _logger.LogInformation("Authorization check for {RequestName}", requestName);
            // Check if the user is authenticated
            if (!_currentUser.IsAuthenticated)
            {
                _logger.LogWarning("Authorization failed for {RequestName}: User is not authenticated", requestName);
                // Use Result for expected failure
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var dataType = typeof(TResponse).GetGenericArguments()[0];
                    var resultType = typeof(Result<>).MakeGenericType(dataType);
                    var failureMethod = resultType.GetMethod("Failure", new[] { typeof(Error) });
                    if (failureMethod != null)
                    {
                        var error = new Error("Auth.Unauthorized", "User is not authenticated.");
                        var result = failureMethod.Invoke(null, new object[] { error });
                        return (TResponse)result!;
                    }
                }
                // Fallback for non-Result responses
                throw new AuthorizationException("User is not authenticated.");
            }
            // Check if the user has the required permissions
            var requiredPermissions = requireAuth.RequiredPermissions;
            if (requiredPermissions.Length > 0 && !_currentUser.HasPermissions(requiredPermissions))
            {
                _logger.LogWarning("Authorization failed for {RequestName}: User does not have required permissions {Permissions}",
                    requestName, string.Join(", ", requiredPermissions));
                // Use Result for expected failure
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var dataType = typeof(TResponse).GetGenericArguments()[0];
                    var resultType = typeof(Result<>).MakeGenericType(dataType);
                    var failureMethod = resultType.GetMethod("Failure", new[] { typeof(Error) });
                    if (failureMethod != null)
                    {
                        var error = new Error("Auth.Forbidden", "User does not have the required permissions.");
                        var result = failureMethod.Invoke(null, new object[] { error });
                        return (TResponse)result!;
                    }
                }
                // Fallback for non-Result responses
                throw new AuthorizationException("User does not have the required permissions.");
            }
            _logger.LogInformation("Authorization succeeded for {RequestName}", requestName);
        }
        // Continue to the next behavior in the pipeline
        return await next();
    }
}
