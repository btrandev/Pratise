using AdminService.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Features.Tenants.GetById;

// GetTenantByIdEndpoint is now defined as extension methods for IEndpointRouteBuilder
public static class GetTenantByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetTenantByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/tenants/{id:guid}", 
            [Authorize]
            async (IMediator mediator, Guid id) =>
            {
                var result = await mediator.Send(new GetTenantByIdQuery(id));
                
                if (result.IsSuccess)
                    return Results.Ok(result.Data);
                
                return Results.Problem(
                    detail: string.Join(", ", result.Errors.Select(e => e.ToString())),
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Tenant not found");
            })
            .WithName("GetTenantById")
            .WithDescription("Retrieves a tenant by its ID")
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
            
        return endpoints;
    }
}
