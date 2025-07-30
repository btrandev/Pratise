using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

public class TenantRepository : BaseRepository<Tenant>, ITenantRepository
{
    public TenantRepository(AdminServiceDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Code == code);
    }

    public async Task<Tenant?> GetByDomainAsync(string domain)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Domain == domain);
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenantsAsync()
    {
        return await _dbSet.Where(t => t.IsActive).ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _dbSet.AnyAsync(t => t.Code == code);
    }

    public async Task<bool> ExistsByDomainAsync(string domain)
    {
        return await _dbSet.AnyAsync(t => t.Domain == domain);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var tenant = await GetByIdAsync(id);
        if (tenant != null)
        {
            tenant.IsDeleted = true;
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.UpdatedById = new Guid("00000000-0000-0000-0000-000000000001");
            tenant.UpdatedByName = "system";
            await _context.SaveChangesAsync();
        }
    }
}
