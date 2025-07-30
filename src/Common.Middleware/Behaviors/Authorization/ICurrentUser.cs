using System;

namespace Common.Middleware.Authorization;

/// <summary>
/// Provides information about the current user
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }
    string? Username { get; }
    Guid? TenantId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool HasPermissions(params string[] permissions);
}
