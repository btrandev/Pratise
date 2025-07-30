using AdminService.Common.Authorization;
using Common.Middleware.Authorization;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.GetPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IRequest<Result<GetUserPermissionsResponse>>, IRequireAuthorization
{
    public string[] RequiredPermissions => new[] { Permissions.Users.View };
}
