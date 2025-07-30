using AdminService.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Features.Users.GetById;

// GetUserByIdEndpoint is now defined as extension methods for IEndpointRouteBuilder
public static class GetUserByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users/{id:guid}", 
            [Authorize]
            async (IMediator mediator, Guid id) =>
            {
                var result = await mediator.Send(new GetUserByIdQuery(id));
                
                if (result.IsSuccess)
                    return Results.Ok(result.Data);
                
                return Results.Problem(
                    detail: string.Join(", ", result.Errors.Select(e => e.ToString())),
                    statusCode: StatusCodes.Status404NotFound,
                    title: "User not found");
            })
            .WithName("GetUserById")
            .WithDescription("Retrieves a user by their ID")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            
        return endpoints;
    }
}
