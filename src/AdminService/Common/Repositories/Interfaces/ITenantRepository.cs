using AdminService.Domain.Entities;

namespace AdminService.Domain.Repositories;

public interface ITenantRepository : IBaseRepository<Tenant>
{
    Task<Tenant?> GetByCodeAsync(string code);
    Task<Tenant?> GetByDomainAsync(string domain);
    Task<IEnumerable<Tenant>> GetActiveTenantsAsync();
    Task<bool> ExistsByCodeAsync(string code);
    Task<bool> ExistsByDomainAsync(string domain);
}
