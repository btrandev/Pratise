using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring response compression in ASP.NET Core applications
/// </summary>
public static class CompressionExtensions
{
    /// <summary>
    /// Adds response compression services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        return services.AddResponseCompression(new CompressionOptions());
    }

    /// <summary>
    /// Adds response compression services to the service collection with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">Options for configuring compression</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services, CompressionOptions options)
    {
        services.AddResponseCompression(compressionOptions =>
        {
            compressionOptions.EnableForHttps = options.EnableForHttps;
            
            if (options.UseGzip)
            {
                compressionOptions.Providers.Add<GzipCompressionProvider>();
            }
            
            if (options.UseBrotli)
            {
                compressionOptions.Providers.Add<BrotliCompressionProvider>();
            }
            
            compressionOptions.MimeTypes = options.MimeTypes;
        });
        
        if (options.UseGzip)
        {
            services.Configure<GzipCompressionProviderOptions>(providerOptions =>
            {
                providerOptions.Level = options.CompressionLevel;
            });
        }
        
        if (options.UseBrotli)
        {
            services.Configure<BrotliCompressionProviderOptions>(providerOptions =>
            {
                providerOptions.Level = options.CompressionLevel;
            });
        }
        
        return services;
    }
    
    /// <summary>
    /// Adds a default response compression configuration to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDefaultResponseCompression(this IServiceCollection services)
    {
        var options = new CompressionOptions
        {
            CompressionLevel = CompressionLevel.Fastest
        };
        
        return services.AddResponseCompression(options);
    }
    
    /// <summary>
    /// Adds a high-compression response compression configuration to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHighCompressionResponse(this IServiceCollection services)
    {
        var options = new CompressionOptions
        {
            CompressionLevel = CompressionLevel.Optimal
        };
        
        return services.AddResponseCompression(options);
    }
    
    /// <summary>
    /// Adds the response compression middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ResponseCompressionMiddleware>();
    }
}
