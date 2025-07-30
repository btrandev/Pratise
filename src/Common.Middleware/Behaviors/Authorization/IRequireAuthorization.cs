namespace Common.Middleware.Authorization;

/// <summary>
/// Marks a request as requiring specific permissions to execute
/// </summary>
public interface IRequireAuthorization
{
    /// <summary>
    /// Gets the permissions required to execute this request
    /// </summary>
    string[] RequiredPermissions { get; }
}
