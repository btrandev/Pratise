using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring response caching middleware
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds response caching services to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="CachingOptions"/></param>
    /// <returns>A reference to the <paramref name="services"/> after the operation has completed</returns>
    public static IServiceCollection AddResponseCaching(
        this IServiceCollection services,
        Action<CachingOptions>? configureOptions = null)
    {
        // Configure options if provided
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        // Get the options to configure caching
        var options = services.BuildServiceProvider()
            .GetService<IOptions<CachingOptions>>()?.Value
            ?? new CachingOptions();
        
        // Add response caching services
        if (options.UseCachingService)
        {
            services.AddResponseCaching(opts =>
            {
                opts.SizeLimit = options.SizeLimitInBytes;
                opts.MaximumBodySize = options.MaximumBodySizeInBytes;
                opts.UseCaseSensitivePaths = false;
            });
        }
        
        // Add cache profiles to MVC if available
        var mvcBuilder = services.AddControllersWithViews();
        if (mvcBuilder != null && options.CacheProfiles.Any())
        {
            mvcBuilder.AddMvcOptions(opts =>
            {
                foreach (var profile in options.CacheProfiles)
                {
                    var cacheProfile = profile.Value;
                    opts.CacheProfiles[profile.Key] = new Microsoft.AspNetCore.Mvc.CacheProfile
                    {
                        Duration = cacheProfile.Duration,
                        Location = Enum.TryParse<Microsoft.AspNetCore.Mvc.ResponseCacheLocation>(cacheProfile.Location, out var location)
                            ? location
                            : Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Any,
                        NoStore = cacheProfile.NoStore,
                        VaryByQueryKeys = cacheProfile.VaryByQueryKeys
                    };
                }
            });
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds the response caching middleware to the specified <see cref="IApplicationBuilder"/>
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to</param>
    /// <returns>A reference to the <paramref name="app"/> after the operation has completed</returns>
    public static IApplicationBuilder UseResponseCaching(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<IOptions<CachingOptions>>()?.Value
            ?? new CachingOptions();

        if (options.UseCachingMiddleware)
        {
            // Call the built-in middleware directly to avoid recursion
            app.UseMiddleware<ResponseCachingMiddleware>();
        }

        return app;
    }
}
