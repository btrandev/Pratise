using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Repositories;

namespace AdminService.Infrastructure.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}