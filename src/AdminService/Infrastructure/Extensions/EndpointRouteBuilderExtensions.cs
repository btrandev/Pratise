using AdminService.Features.Auth.Login;
using AdminService.Features.Tenants.GetById;
using AdminService.Features.Users.Create;
using AdminService.Features.Users.GetById;
using AdminService.Features.Users.GetPermissions;
using AdminService.Features.Users.UpdatePermissions;

namespace AdminService.Infrastructure.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        // Map Controllers (for any remaining controller-based endpoints)
        app.MapControllers();

        // Map Minimal API endpoints
        app.MapLoginEndpoints();
        app.MapGetTenantByIdEndpoint();
        app.MapGetUserByIdEndpoint();
        app.MapCreateUserEndpoint();
        app.MapUpdateUserPermissionsEndpoint();
        app.MapGetUserPermissionsEndpoint();
    }
}