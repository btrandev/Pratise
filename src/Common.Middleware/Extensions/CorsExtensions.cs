using Common.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring CORS middleware
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS services to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="CorsOptions"/></param>
    /// <returns>A reference to the <paramref name="services"/> after the operation has completed</returns>
    public static IServiceCollection AddCors(this IServiceCollection services, Action<CorsOptions>? configureOptions = null)
    {
        // Configure options if provided
        var corsOptions = new CorsOptions();
        if (configureOptions != null)
        {
            configureOptions(corsOptions);
        }

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                var policyBuilder = policy.AllowAnyMethod();

                // If credentials are allowed, do NOT allow any origin
                if (corsOptions.AllowCredentials)
                {
                    if (corsOptions.AllowedOrigins.Any())
                    {
                        policyBuilder.WithOrigins(corsOptions.AllowedOrigins.ToArray());
                        policyBuilder.AllowCredentials();
                    }
                    // else: do not allow credentials if no explicit origins are set
                }
                else
                {
                    if (corsOptions.AllowAnyOrigin)
                    {
                        policyBuilder.AllowAnyOrigin();
                    }
                    else if (corsOptions.AllowedOrigins.Any())
                    {
                        policyBuilder.WithOrigins(corsOptions.AllowedOrigins.ToArray());
                    }
                }

                if (corsOptions.AllowAnyHeader)
                {
                    policyBuilder.AllowAnyHeader();
                }
                else if (corsOptions.AllowedHeaders.Any())
                {
                    policyBuilder.WithHeaders(corsOptions.AllowedHeaders.ToArray());
                }

                if (corsOptions.ExposedHeaders.Any())
                {
                    policyBuilder.WithExposedHeaders(corsOptions.ExposedHeaders.ToArray());
                }
            });
        });

        return services;
    }
}
