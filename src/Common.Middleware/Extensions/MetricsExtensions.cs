using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring Prometheus metrics in ASP.NET Core applications
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Adds Prometheus metrics services to the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional delegate to configure metrics options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPrometheusMetrics(
        this IServiceCollection services,
        Action<MetricsOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds Prometheus metrics middleware to the application based on configured options
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UsePrometheusMetrics(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<IOptions<MetricsOptions>>()?.Value
            ?? new MetricsOptions();
        
        // Expose the Prometheus metrics at the specified endpoint
        app.UseMetricServer(options.MetricsEndpoint);
        
        // Handle process metrics
        if (options.IncludeProcessMetrics)
        {
            if (options.SuppressDefaultMetrics)
            {
                Metrics.SuppressDefaultMetrics();
            }
            
            if (options.CustomLabels.Any())
            {
                Metrics.DefaultRegistry.SetStaticLabels(options.CustomLabels);
            }
            else
            {
                // Set default service name if no custom labels are provided
                Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
                {
                    { "service", Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown" }
                });
            }
        }
        
        // Capture HTTP metrics if requested
        if (options.IncludeHttpMetrics)
        {
            app.UseHttpMetrics(httpOptions =>
            {
                if (options.CustomLabels.Any())
                {
                    foreach (var label in options.CustomLabels)
                    {
                        httpOptions.AddCustomLabel(label.Key, context => label.Value);
                    }
                }
            });
        }
        
        return app;
    }
}
