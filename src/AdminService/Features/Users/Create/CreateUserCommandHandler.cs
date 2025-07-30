using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Features.Users.GetById;
using AdminService.Infrastructure.Authorization;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.Create;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly PermissionManager _permissionManager;
    private readonly ICurrentUser _currentUser;

    public CreateUserCommandHandler(
        IUserRepository userRepository, 
        ITenantRepository tenantRepository,
        PermissionManager permissionManager,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _permissionManager = permissionManager;
        _currentUser = currentUser;
    }

    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Payload;
        
        // Check if tenant exists
        if (request.TenantId != Guid.Empty)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                return Result<UserResponse>.Failure($"Tenant with ID '{request.TenantId}' not found.");
            }
        }

        // Check if user with same email exists
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            return Result<UserResponse>.Failure($"User with email '{request.Email}' already exists.");
        }

        // Check if user with same username exists
        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            return Result<UserResponse>.Failure($"User with username '{request.Username}' already exists.");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            EmailConfirmed = false,
            Role = request.Role,
            TenantId = request.TenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = GetCurrentUserId() ?? new Guid("00000000-0000-0000-0000-000000000001"),
            CreatedByName = "system"
        };

        // Save user to database
        await _userRepository.AddAsync(user);
        
        // Sync role-based permissions
        await _permissionManager.SyncPermissionsForUserAsync(user);

        // Return response
        var response = new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Username,
            user.PhoneNumber,
            user.IsActive,
            user.EmailConfirmed,
            user.LastLoginAt,
            user.Role,
            user.TenantId,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedById,
            user.UpdatedById,
            user.CreatedByName,
            user.UpdatedByName
        );

        return Result<UserResponse>.Success(response);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    private Guid? GetCurrentUserId()
    {
        return _currentUser.Id;
    }
}
