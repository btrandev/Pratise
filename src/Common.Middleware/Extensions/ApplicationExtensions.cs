using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring all standard middleware components at once
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Adds all standard middleware services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStandardMiddlewareServices(this IServiceCollection services)
    {
        return services.AddStandardMiddlewareServices(new ApplicationOptions());
    }
    
    /// <summary>
    /// Adds all standard middleware services to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Options for configuring middleware services</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStandardMiddlewareServices(this IServiceCollection services, ApplicationOptions options)
    {
        // Configure default options for all middleware
        services.Configure<ApplicationOptions>(opt => 
        {
            opt.UseCors = options.UseCors;
            opt.UseCompression = options.UseCompression;
            opt.UseSecurityHeaders = options.UseSecurityHeaders;
            opt.UseRequestLogging = options.UseRequestLogging;
            opt.UseMetrics = options.UseMetrics;
            opt.UseResponseCaching = options.UseResponseCaching;
            opt.UseRateLimiting = options.UseRateLimiting;
            opt.UseAntiforgery = options.UseAntiforgery;
            opt.UseHealthChecks = options.UseHealthChecks;
            opt.UseGlobalExceptionHandling = options.UseGlobalExceptionHandling;
        });

        if(options.UseRequestLogging)
        {
            services.AddRequestLogging();
        }
        
        if (options.UseCors)
        {
            services.AddCors();
        }
        
        if (options.UseCompression)
        {
            services.AddResponseCompression();
        }
        
        // if (options.UseHealthChecks)
        // {
        //     services.AddHealthChecks();
        // }
        
        if (options.UseResponseCaching)
        {
            services.AddResponseCaching();
        }
        
        if (options.UseRateLimiting)
        {
            services.AddRateLimiting();
        }
        
        // if (options.UseAntiforgery)
        // {
        //     services.AddAntiforgery();
        // }
        
        if (options.UseSecurityHeaders)
        {
            services.AddSecurityHeaders();
        }
        
        if (options.UseGlobalExceptionHandling)
        {
            services.AddGlobalExceptionHandling();
        }
        
        // Add Azure services if enabled
        // if (options.UseApplicationInsights)
        // {
        //     services.AddApplicationInsights(options.ApplicationInsightsOptions);
        // }
        
        return services;
    }
    
    /// <summary>
    /// Configures the HTTP request pipeline with all standard middleware components
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseStandardMiddleware(this IApplicationBuilder app)
    {
        return app.UseStandardMiddleware(new ApplicationOptions());
    }
    
    /// <summary>
    /// Configures the HTTP request pipeline with all standard middleware components with custom options
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="options">Options for configuring the middleware pipeline</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseStandardMiddleware(this IApplicationBuilder app, ApplicationOptions options)
    {
        // Configure global exception handling first
        if (options.UseGlobalExceptionHandling)
        {
            app.UseGlobalExceptionHandling();
        }
        
        // Add request logging early in the pipeline
        if (options.UseRequestLogging)
        {
            app.UseRequestLogging();
        }
        
        // Add rate limiting early
        if (options.UseRateLimiting)
        {
            app.UseRateLimiting();
        }
        
        // Add security headers
        if (options.UseSecurityHeaders)
        {
            app.UseSecurityHeaders();
        }
        
        // Add response caching before compression
        if (options.UseResponseCaching)
        {
            app.UseResponseCaching();
        }
        
        // Add compression before serving static files
        if (options.UseCompression)
        {
            app.UseResponseCompression();
        }
        
        // Add CORS after compression but before endpoints
        if (options.UseCors)
        {
            app.UseCors("DefaultPolicy");
        }
        
        // Add health checks
        // if (options.UseHealthChecks)
        // {
        //     app.UseHealthChecks();
        // }
        
        // Add metrics near the end of the pipeline
        if (options.UseMetrics)
        {
            app.UsePrometheusMetrics();
        }

        // Register in Program.cs
        app.UseMiddleware<CorrelationIdMiddleware>();
        
        return app;
    }
}
