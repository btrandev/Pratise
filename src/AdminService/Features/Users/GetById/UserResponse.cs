namespace AdminService.Features.Users.GetById;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Username,
    string? PhoneNumber,
    bool IsActive,
    bool EmailConfirmed,
    DateTime? LastLoginAt,
    string? Role,
    Guid TenantId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid CreatedById,
    Guid? UpdatedById,
    string? CreatedByName,
    string? UpdatedByName
);
