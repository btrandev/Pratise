using AdminService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace IntegrationTest.Postman.Framework.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove all registered DbContextOptions to avoid conflicts
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AdminServiceDbContext>));

                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }
                
                // Remove the DbContextOptions<T> registration
                var dbContextOptionsBuilderDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptionsBuilder<AdminServiceDbContext>));
                    
                if (dbContextOptionsBuilderDescriptor != null)
                {
                    services.Remove(dbContextOptionsBuilderDescriptor);
                }

                // Also remove any general DbContextOptions if present
                var dbContextOptions = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions))
                    .ToList();
                    
                foreach (var option in dbContextOptions)
                {
                    services.Remove(option);
                }

                // Add in-memory database for testing
                services.AddDbContext<AdminServiceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseApplicationServiceProvider(null); // Don't use an external service provider
                });

                // Don't build a service provider here as it can cause circular dependency issues
                // We'll seed the database after the test host has started
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Initialize and seed the database
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    var context = serviceProvider.GetRequiredService<AdminServiceDbContext>();
                    
                    // Ensure database is created
                    context.Database.EnsureCreated();
                    
                    // Seed test data
                    SeedTestData(context);
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            return host;
        }

        private void SeedTestData(AdminServiceDbContext context)
        {
            // Seed admin user for testing
            if (!context.Users.Any())
            {
                context.Users.Add(new AdminService.Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@system.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Make sure to add BCrypt.Net-Next NuGet package
                    FirstName = "Admin",
                    LastName = "User",
                    TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                });
                
                // Add tenant
                if (!context.Tenants.Any())
                {
                    context.Tenants.Add(new AdminService.Domain.Entities.Tenant
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "Default Tenant",
                        Description = "Default tenant for testing"
                    });
                }
                
                context.SaveChanges();
            }
        }
    }
}
