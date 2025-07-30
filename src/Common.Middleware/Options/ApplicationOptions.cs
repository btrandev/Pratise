namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring standard middleware services and pipeline
/// </summary>
public class ApplicationOptions
{
    /// <summary>
    /// Gets or sets whether to use CORS middleware
    /// </summary>
    public bool UseCors { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use compression middleware
    /// </summary>
    public bool UseCompression { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use security headers middleware
    /// </summary>
    public bool UseSecurityHeaders { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use request logging middleware
    /// </summary>
    public bool UseRequestLogging { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use metrics middleware
    /// </summary>
    public bool UseMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use response caching middleware
    /// </summary>
    public bool UseResponseCaching { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use rate limiting middleware
    /// </summary>
    public bool UseRateLimiting { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use antiforgery middleware
    /// </summary>
    public bool UseAntiforgery { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use health checks middleware
    /// </summary>
    public bool UseHealthChecks { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use global exception handling middleware
    /// </summary>
    public bool UseGlobalExceptionHandling { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use Azure Application Insights
    /// </summary>
    public bool UseApplicationInsights { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to use Azure Key Vault for secrets
    /// </summary>
    public bool UseKeyVault { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to use Azure Blob Storage for logging
    /// </summary>
    public bool UseBlobStorageLogging { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to use Azure App Service integration
    /// </summary>
    public bool UseAppServiceIntegration { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the App Service options
    /// </summary>
    public AppServiceOptions AppServiceOptions { get; set; } = new AppServiceOptions();
}
