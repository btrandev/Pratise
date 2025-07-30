using AdminService.Common.Results;
using AdminService.Domain.Repositories;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Tenants.GetById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantResponse>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id);
        if (tenant == null)
        {
            return Result<TenantResponse>.Failure(new Error("Tenant.NotFound", $"Tenant with ID '{request.Id}' not found."));
        }

        var tenantResponse = new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Code,
            tenant.Description,
            tenant.Domain,
            tenant.IsActive,
            tenant.SubscriptionPlan,
            tenant.CreatedAt,
            tenant.UpdatedAt,
            tenant.CreatedById,
            tenant.UpdatedById,
            tenant.CreatedByName,
            tenant.UpdatedByName
        );
        
        return Result<TenantResponse>.Success(tenantResponse);
    }
}
