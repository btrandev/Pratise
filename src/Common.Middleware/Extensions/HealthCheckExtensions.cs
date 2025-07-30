using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring health checks in ASP.NET Core applications
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health checks to the service collection with customizable options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional delegate to configure health check options</param>
    /// <returns>The IHealthChecksBuilder for adding more health checks</returns>
    public static IHealthChecksBuilder AddHealthChecks(
        this IServiceCollection services, 
        Action<Common.Middleware.Options.HealthCheckOptions>? configureOptions = null)
    {
        // Configure options if provided
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        var builder = services.AddHealthChecks();
        
        // Get the options to determine if we need Kubernetes probes
        var options = services.BuildServiceProvider().GetService<IOptions<Common.Middleware.Options.HealthCheckOptions>>()?.Value
            ?? new Common.Middleware.Options.HealthCheckOptions();
            
        if (options.UseKubernetesProbes)
        {
            builder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live", "ready" });
        }
        
        return builder;
    }

    /// <summary>
    /// Maps health check endpoints based on the configured options
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<IOptions<Common.Middleware.Options.HealthCheckOptions>>()?.Value
            ?? new Common.Middleware.Options.HealthCheckOptions();
            
        if (options.UseKubernetesProbes)
        {
            // Map Kubernetes readiness probe
            app.UseHealthChecks(options.ReadinessPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            
            // Map Kubernetes liveness probe
            app.UseHealthChecks(options.LivenessPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });
        }
        else
        {
            // Map simple health check endpoint
            app.UseHealthChecks(options.HealthCheckPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions());
        }
        
        return app;
    }
}
