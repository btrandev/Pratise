using FluentValidation;

namespace AdminService.Features.Users.UpdatePermissions;

public class UpdateUserPermissionsValidator : AbstractValidator<UpdateUserPermissionsRequest>
{
    public UpdateUserPermissionsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
            
        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions array cannot be null.");
    }
}
