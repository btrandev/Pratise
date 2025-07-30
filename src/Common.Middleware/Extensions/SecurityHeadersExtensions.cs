using Common.Middleware.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extension methods for adding security headers middleware to an application
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Adds the security headers middleware to the specified <see cref="IApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>A reference to the <paramref name="builder"/> after the operation has completed.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseSecurityHeaders(new SecurityHeadersOptions());
    }

    /// <summary>
    /// Adds the security headers middleware to the specified <see cref="IApplicationBuilder"/> with the specified options.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <param name="options">The options to configure the middleware.</param>
    /// <returns>A reference to the <paramref name="builder"/> after the operation has completed.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, SecurityHeadersOptions options)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
    
    /// <summary>
    /// Adds security headers services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        return services.AddSecurityHeaders(new SecurityHeadersOptions());
    }
    
    /// <summary>
    /// Adds security headers services to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The options to configure the security headers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services, SecurityHeadersOptions options)
    {
        services.AddSingleton(options);
        return services;
    }
}
