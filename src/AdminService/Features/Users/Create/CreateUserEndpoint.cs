using AdminService.Common.Results;
using AdminService.Features.Users.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Features.Users.Create;

// CreateUserEndpoint is now defined as extension methods for IEndpointRouteBuilder
public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/users",
            async (IMediator mediator, [FromBody] CreateUserRequest request) =>
            {
                var result = await mediator.Send(new CreateUserCommand(request));
                
                if (!result.IsSuccess)
                    return Results.Problem(
                        detail: string.Join(", ", result.Errors.Select(e => e.ToString())),
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "User creation failed");
                
                // Ensure data is not null
                if (result.Data == null)
                    return Results.Problem(
                        detail: "User created but response data is null",
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Server error");

                return Results.CreatedAtRoute(
                    "GetUserById",
                    new { id = result.Data.Id },
                    result.Data);
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
            
        return endpoints;
    }
}
