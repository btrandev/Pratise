using AdminService.Common.Authorization;
using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;

namespace AdminService.Infrastructure.Authorization;

/// <summary>
/// Utility class for managing permissions
/// </summary>
public class PermissionManager
{
    private readonly IUserRepository _userRepository;
    
    public PermissionManager(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    /// <summary>
    /// Sync user permissions based on role
    /// </summary>
    public async Task SyncPermissionsForUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        // Get permissions based on role
        string[] rolePermissions = user.Role?.ToLowerInvariant() switch
        {
            "admin" => Permissions.Roles.Admin,
            "tenantadmin" => Permissions.Roles.TenantAdmin,
            _ => Permissions.Roles.StandardUser // Default to standard permissions
        };
        
        // Remove existing permission claims
        var existingPermissionClaims = user.Claims
            .Where(c => c.ClaimType == "permission")
            .ToList();
            
        foreach (var claim in existingPermissionClaims)
        {
            user.Claims.Remove(claim);
        }
        
        // Add new permission claims
        foreach (var permission in rolePermissions)
        {
            user.Claims.Add(new UserClaim
            {
                ClaimType = "permission",
                ClaimValue = permission,
                UserId = user.Id,
                CreatedById = user.CreatedById,
                CreatedByName = user.CreatedByName
            });
        }
        
        await _userRepository.UpdateAsync(user);
    }
    
    /// <summary>
    /// Add a specific permission to a user
    /// </summary>
    public async Task AddPermissionToUserAsync(User user, string permission)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        // Check if the user already has this permission
        if (user.Claims.Any(c => c.ClaimType == "permission" && c.ClaimValue == permission))
            return;
            
        // Add permission
        user.Claims.Add(new UserClaim
        {
            ClaimType = "permission",
            ClaimValue = permission,
            UserId = user.Id,
            CreatedById = user.CreatedById,
            CreatedByName = user.CreatedByName
        });
        
        await _userRepository.UpdateAsync(user);
    }
    
    /// <summary>
    /// Remove a specific permission from a user
    /// </summary>
    public async Task RemovePermissionFromUserAsync(User user, string permission)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
        var claim = user.Claims.FirstOrDefault(c => 
            c.ClaimType == "permission" && c.ClaimValue == permission);
            
        if (claim != null)
        {
            user.Claims.Remove(claim);
            await _userRepository.UpdateAsync(user);
        }
    }
}
