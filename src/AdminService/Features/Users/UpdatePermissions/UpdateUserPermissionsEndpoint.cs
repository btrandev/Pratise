using AdminService.Common.Results;
using Common.Middleware.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminService.Features.Users.UpdatePermissions;

/// <summary>
/// Endpoint for updating user permissions
/// </summary>
public static class UpdateUserPermissionsEndpoint
{
    public static void MapUpdateUserPermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/users/{userId:guid}/permissions", async (
            Guid userId,
            UpdateUserPermissionsRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            // Set the user ID from the route
            request.UserId = userId;
            
            var command = new UpdateUserPermissionsCommand(request);
            var result = await mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }
            
            return Results.BadRequest(result);
        })
        .WithName("UpdateUserPermissions")
        .RequireAuthorization()
        .RequireRateLimitingPerSecond(); // Apply 1 request per second rate limiting
    }
}
