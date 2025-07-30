using Common.Middleware.Middleware;
using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions methods for configuring exception handling middleware
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Adds exception handling services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        return services.AddGlobalExceptionHandling(new ExceptionHandlingOptions());
    }

    /// <summary>
    /// Adds exception handling services to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The options for configuring exception handling</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services, ExceptionHandlingOptions options)
    {
        services.AddSingleton(options);
        return services;
    }

    /// <summary>
    /// Configures global exception handling for the application
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
