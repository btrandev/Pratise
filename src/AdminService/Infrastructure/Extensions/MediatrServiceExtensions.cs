using System.Reflection;
using Common.Middleware.Authorization;
using Common.Middleware.Behaviors;
using Common.Middleware.Extensions;
using Common.Middleware.Options;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdminService.Infrastructure.Extensions;

public static class MediatrServiceExtensions
{
    public static IServiceCollection AddMediatrServices(this IServiceCollection services)
    {
        // Get configuration
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        
        // Add MediatR with standard behaviors
        var options = new MediatROptions
        {
            UseValidationBehavior = true,  // Explicitly enable validation
            UseLoggingBehavior = true
        };
        services.AddMediatRWithBehaviors(options, Assembly.GetExecutingAssembly());
        
        // Configure logging options based on configuration
        services.ConfigureMediatRLogging(options =>
        {
            // Bind from configuration if available
            configuration.GetSection("LoggingOptions").Bind(options);
            
            // Ensure payload logging is disabled in production
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
            {
                options.LogPayloads = false;
            }
        });

        // Register additional MediatR behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PermissionRequirementBehavior<,>));

        return services;
    }
}