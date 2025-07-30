using FluentValidation;

namespace AdminService.Features.Auth.Login;

/// <summary>
/// Validator for LoginCommand - delegates to the LoginRequestValidator
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Login request is required")
            .SetValidator(new LoginRequestValidator());
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Matches(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,}$").WithMessage("Please enter a valid email address with a proper domain");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
    
    // Custom validation method removed since we're using a regex pattern directly
}
