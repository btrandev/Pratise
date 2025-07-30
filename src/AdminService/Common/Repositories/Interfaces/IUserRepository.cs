using AdminService.Domain.Entities;

namespace AdminService.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<User>> GetByTenantIdPagedAsync(Guid tenantId, int page, int pageSize);
    Task<int> GetTotalCountByTenantIdAsync(Guid tenantId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId);
}
