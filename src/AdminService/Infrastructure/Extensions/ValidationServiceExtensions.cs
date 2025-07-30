using System.Reflection;
using FluentValidation;

namespace AdminService.Infrastructure.Extensions;

public static class ValidationServiceExtensions
{
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Add FluentValidation validators from this assembly
        // This automatically registers all concrete validator implementations with the DI container
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}