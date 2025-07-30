using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Common.Middleware.Authorization;

/// <summary>
/// Implementation of ICurrentUser that gets user information from HttpContext
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId);
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public Guid? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id") is string tid && Guid.TryParse(tid, out var guid) ? guid : (Guid?)null;

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermissions(params string[] permissions)
    {
        var userClaims = _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet() ?? new HashSet<string>();
        return permissions.All(p => userClaims.Contains(p));
    }
}
