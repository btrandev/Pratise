using Common.Middleware.Logging;
using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extension methods for adding request logging middleware to an application
/// </summary>
public static class RequestLoggingExtensions
{
    /// <summary>
    /// Adds the request logging middleware to the specified <see cref="IApplicationBuilder"/>
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to</param>
    /// <returns>A reference to the <paramref name="builder"/> after the operation has completed</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
    
    /// <summary>
    /// Adds request logging services to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="RequestLoggingOptions"/></param>
    /// <returns>A reference to the <paramref name="services"/> after the operation has completed</returns>
    public static IServiceCollection AddRequestLogging(this IServiceCollection services, Action<RequestLoggingOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        return services;
    }
}
