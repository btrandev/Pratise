using AdminService.Common.Authorization;
using AdminService.Common.Results;
using AdminService.Domain.Repositories;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.GetPermissions;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<GetUserPermissionsResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public GetUserPermissionsQueryHandler(
        IUserRepository userRepository, 
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<GetUserPermissionsResponse>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        var userId = query.UserId;
        
        // Get user by ID
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<GetUserPermissionsResponse>.Failure($"User with ID '{userId}' not found.");
        }
        
        // Verify tenant access if the current user is not an admin
        if (_currentUser.Role != "Admin" && _currentUser.TenantId != user.TenantId)
        {
            return Result<GetUserPermissionsResponse>.Failure("You don't have permission to view this user's permissions.");
        }
        
        // Get permissions from claims
        var permissions = user.Claims
            .Where(c => c.ClaimType == "permission")
            .Select(c => c.ClaimValue)
            .ToArray();
        
        var response = new GetUserPermissionsResponse
        {
            UserId = userId,
            Permissions = permissions
        };
        
        return Result<GetUserPermissionsResponse>.Success(response);
    }
}
