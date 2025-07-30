using Common.Middleware.Behaviors;
using Common.Middleware.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for registering MediatR behaviors
/// </summary>
public static class MediatRExtensions
{
    /// <summary>
    /// Adds the standard set of MediatR behaviors to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRBehaviors(this IServiceCollection services)
    {
        return services.AddMediatRBehaviors(new MediatROptions());
    }
    
    /// <summary>
    /// Adds the standard set of MediatR behaviors to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Options for configuring MediatR behaviors</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRBehaviors(this IServiceCollection services, MediatROptions options)
    {
        if (options.UseValidationBehavior)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
        
        if (options.UseLoggingBehavior)
        {
            // Register logging options if not already registered
            services.TryAddSingleton(new LoggingOptions 
            { 
                LogPayloads = false, // Default to false for production safety
                MaxPayloadSize = 10000 
            });
            
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
        
        return services;
    }
    
    /// <summary>
    /// Configures logging options for MediatR behaviors
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the logging options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigureMediatRLogging(this IServiceCollection services, Action<LoggingOptions> configureOptions)
    {
        services.Configure<LoggingOptions>(options => configureOptions(options));
        return services;
    }
    
    /// <summary>
    /// Adds MediatR with standard behaviors and registers handlers from the specified assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">The assembly containing the handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, Assembly assembly)
    {
        return services.AddMediatRWithBehaviors(new MediatROptions(), assembly);
    }
    
    /// <summary>
    /// Adds MediatR with standard behaviors and registers handlers from the specified assembly with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Options for configuring MediatR behaviors</param>
    /// <param name="assembly">The assembly containing the handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, MediatROptions options, Assembly assembly)
    {
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        return services.AddMediatRBehaviors(options);
    }
    
    /// <summary>
    /// Adds MediatR with standard behaviors and registers handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies containing the handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddMediatRWithBehaviors(new MediatROptions(), assemblies);
    }
    
    /// <summary>
    /// Adds MediatR with standard behaviors and registers handlers from the specified assemblies with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Options for configuring MediatR behaviors</param>
    /// <param name="assemblies">The assemblies containing the handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, MediatROptions options, params Assembly[] assemblies)
    {
        services.AddMediatR(cfg => 
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });
        
        return services.AddMediatRBehaviors(options);
    }
}
