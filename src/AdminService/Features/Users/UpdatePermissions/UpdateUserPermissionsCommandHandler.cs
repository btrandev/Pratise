using AdminService.Common.Authorization;
using AdminService.Common.Results;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Authorization;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.UpdatePermissions;

public class UpdateUserPermissionsCommandHandler : IRequestHandler<UpdateUserPermissionsCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly PermissionManager _permissionManager;
    private readonly ICurrentUser _currentUser;

    public UpdateUserPermissionsCommandHandler(
        IUserRepository userRepository,
        PermissionManager permissionManager,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _permissionManager = permissionManager;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateUserPermissionsCommand command, CancellationToken cancellationToken)
    {
        var userId = command.Payload.UserId;
        var permissions = command.Payload.Permissions;
        
        // Get user by ID
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<bool>.Failure($"User with ID '{userId}' not found.");
        }
        
        // Verify tenant access if the current user is not an admin
        if (_currentUser.Role != "Admin" && _currentUser.TenantId != user.TenantId)
        {
            return Result<bool>.Failure("You don't have permission to update this user's permissions.");
        }
        
        try
        {
            // Clear existing permissions
            var existingPermissionClaims = user.Claims
                .Where(c => c.ClaimType == "permission")
                .ToList();
                
            foreach (var claim in existingPermissionClaims)
            {
                user.Claims.Remove(claim);
            }
            
            // Add new permissions
            foreach (var permission in permissions)
            {
                await _permissionManager.AddPermissionToUserAsync(user, permission);
            }
            
            // Update user
            await _userRepository.UpdateAsync(user);
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update user permissions: {ex.Message}");
        }
    }
}
