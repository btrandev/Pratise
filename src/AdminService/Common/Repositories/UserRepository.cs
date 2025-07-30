using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AdminServiceDbContext context) : base(context)
    {
    }
    
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _dbSet.Where(u => u.TenantId == tenantId).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByTenantIdPagedAsync(Guid tenantId, int page, int pageSize)
    {
        return await _dbSet
            .Where(u => u.TenantId == tenantId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountByTenantIdAsync(Guid tenantId)
    {
        return await _dbSet.CountAsync(u => u.TenantId == tenantId);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAndTenantAsync(string email, Guid tenantId)
    {
        return await _dbSet
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedById = new Guid("00000000-0000-0000-0000-000000000001");
            user.UpdatedByName = "system";
            await _context.SaveChangesAsync();
        }
    }
}
