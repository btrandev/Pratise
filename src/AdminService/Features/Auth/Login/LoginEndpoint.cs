using AdminService.Common.Results;
using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Authentication;
using Common.Middleware.Extensions;
using Common.Middleware.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Features.Auth.Login;

// LoginEndpoint is now defined as extension methods for IEndpointRouteBuilder
public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", 
            [AllowAnonymous]
            async (IMediator mediator, [FromBody] LoginRequest request) =>
            {
                var result = await mediator.Send(new LoginCommand(request));

                // Always return the full Result<LoginResponse> object
                return Results.Ok(result);
            })
            .WithName("Login")
            .WithDescription("Authenticates a user and returns access tokens")
            .Produces<Result<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<Result<LoginResponse>>(StatusCodes.Status401Unauthorized)
            .RequireRateLimitingPerSecond(); // Apply 1 request per second rate limiting

        return endpoints;
    }
}
