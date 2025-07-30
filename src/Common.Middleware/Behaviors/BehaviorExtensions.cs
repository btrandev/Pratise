using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Middleware.Behaviors;

/// <summary>
/// Extensions for registering MediatR behaviors
/// </summary>
public static class BehaviorExtensions
{
    /// <summary>
    /// Adds the standard set of MediatR behaviors to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStandardMediatRBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
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
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        return services.AddStandardMediatRBehaviors();
    }
    
    /// <summary>
    /// Adds MediatR with standard behaviors and registers handlers from the specified assemblies
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies containing the handlers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(cfg => 
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });
        
        return services.AddStandardMediatRBehaviors();
    }
}
