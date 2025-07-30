using System.Security.Claims;
using Common.Middleware.Authorization;

namespace AdminService.Tests.Common.Authorization;

/// <summary>
/// Test implementation of ICurrentUser for unit tests
/// </summary>
public class TestCurrentUser : ICurrentUser
{
    private readonly List<Claim> _claims = new();
    private readonly List<string> _permissions = new();
    private readonly string? _role;

    public TestCurrentUser(Guid? id = null, string? username = null, Guid? tenantId = null, string? role = null, bool isAuthenticated = true)
    {
        Id = id;
        Username = username;
        TenantId = tenantId;
        _role = role;
        IsAuthenticated = isAuthenticated;
        
        if (id.HasValue)
        {
            _claims.Add(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
        }
        
        if (username != null)
        {
            _claims.Add(new Claim(ClaimTypes.Name, username));
        }
        
        if (tenantId.HasValue)
        {
            _claims.Add(new Claim("tenantId", tenantId.Value.ToString()));
        }
        
        if (role != null)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    public Guid? Id { get; }
    public string? Username { get; }
    public Guid? TenantId { get; }
    public string? Role => _role;
    public IEnumerable<Claim> Claims => _claims;
    public bool IsAuthenticated { get; }

    public void AddPermission(string permission)
    {
        _permissions.Add(permission);
        _claims.Add(new Claim("permission", permission));
    }
    
    public void AddPermissions(IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            AddPermission(permission);
        }
    }

    public bool HasPermission(string permission)
    {
        if (Role == "Admin")
            return true;
            
        return _permissions.Contains(permission);
    }

    public bool HasPermissions(params string[] permissions)
    {
        return permissions.All(HasPermission);
    }

    public bool IsInRole(string role)
    {
        return Role == role;
    }
}
