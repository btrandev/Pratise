using AdminService.Common.Authorization;
using AdminService.Common.Results;
using AdminService.Features.Users.GetById;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.Create;

public record CreateUserCommand(CreateUserRequest Payload) : IRequest<Result<UserResponse>>, IRequireAuthorization
{
    public string[] RequiredPermissions => new[] { Permissions.Users.Create };
}
