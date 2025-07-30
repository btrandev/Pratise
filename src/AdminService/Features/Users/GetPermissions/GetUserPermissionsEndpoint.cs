using AdminService.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminService.Features.Users.GetPermissions;

/// <summary>
/// Endpoint for getting user permissions
/// </summary>
public static class GetUserPermissionsEndpoint
{
    public static void MapGetUserPermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users/{userId:guid}/permissions", async (
            Guid userId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserPermissionsQuery(userId);
            var result = await mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            
            return Results.BadRequest(result);
        })
        .WithName("GetUserPermissions")
        .RequireAuthorization();
    }
}
