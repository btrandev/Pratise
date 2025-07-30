using Common.Middleware.Attributes;
using FluentValidation;

namespace AdminService.Features.Users.Create;

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    [Sensitive]
    public string Password { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? Role { get; set; }
    
    public Guid TenantId { get; set; } = default;
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
