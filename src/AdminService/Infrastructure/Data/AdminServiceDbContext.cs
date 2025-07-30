using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data;

public class AdminServiceDbContext : DbContext
{
    public AdminServiceDbContext(DbContextOptions<AdminServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserClaim> UserClaims { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SubscriptionPlan).HasMaxLength(20);
            
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Domain).IsUnique();
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.TenantId);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // UserClaim configuration
        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.Claims)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                if (entry.Entity.CreatedById == Guid.Empty)
                {
                    // Using a special GUID to represent system actions
                    entry.Entity.CreatedById = new Guid("00000000-0000-0000-0000-000000000001");
                    entry.Entity.CreatedByName = "system";
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity.UpdatedById == null || entry.Entity.UpdatedById == Guid.Empty)
                {
                    // Using a special GUID to represent system actions
                    entry.Entity.UpdatedById = new Guid("00000000-0000-0000-0000-000000000001");
                    entry.Entity.UpdatedByName = "system";
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
