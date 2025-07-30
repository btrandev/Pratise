using AdminService.Common.Authorization;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.UpdatePermissions;

public record UpdateUserPermissionsCommand(UpdateUserPermissionsRequest Payload) : IRequest<Result<bool>>, IRequireAuthorization
{
    public string[] RequiredPermissions => new[] { Permissions.Users.Update };
}
