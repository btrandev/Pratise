using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Common.Middleware.Extensions;

/// <summary>
/// Extensions for configuring configuration builders with Azure services
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider to the configuration builder
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="keyVaultUri">The Key Vault URI</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddAzureKeyVault(
        this IConfigurationBuilder builder,
        string keyVaultUri)
    {
        return builder.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
    }
    
    /// <summary>
    /// Adds Azure App Configuration provider to the configuration builder
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="endpoint">The App Configuration endpoint</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddAzureAppConfiguration(
        this IConfigurationBuilder builder,
        string endpoint)
    {
        return builder.AddAzureAppConfiguration(options =>
        {
            // Use managed identity for authentication
            options.Connect(new Uri(endpoint), new DefaultAzureCredential());
            
            // Configure label filter to match environment
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            options.Select("*", env);
            options.Select("*", string.Empty);  // Add unlabeled keys
            
            // Configure to refresh configuration if sentinel key changes
            options.ConfigureRefresh(refresh =>
            {
                refresh.Register("Sentinel", refreshAll: true)
                    .SetCacheExpiration(TimeSpan.FromMinutes(5));
            });
        });
    }
}
