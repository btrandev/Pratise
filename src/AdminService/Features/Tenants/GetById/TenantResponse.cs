namespace AdminService.Features.Tenants.GetById;

public record TenantResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string Domain,
    bool IsActive,
    string? SubscriptionPlan,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid CreatedById,
    Guid? UpdatedById,
    string? CreatedByName,
    string? UpdatedByName
);
