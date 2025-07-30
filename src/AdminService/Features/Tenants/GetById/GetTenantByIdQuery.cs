using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Tenants.GetById;

public record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantResponse>>;
