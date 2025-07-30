namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring Azure App Service integration
/// </summary>
public class AppServiceOptions
{
    /// <summary>
    /// Gets or sets whether to forward HTTP headers from Azure infrastructure
    /// </summary>
    /// <remarks>
    /// When enabled, headers such as X-Forwarded-For, X-Forwarded-Proto, etc.
    /// are respected when determining client information.
    /// </remarks>
    public bool ForwardHttpHeaders { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to configure HTTPS redirection.
    /// </summary>
    /// <remarks>
    /// When true, HTTP requests are redirected to HTTPS.
    /// This should typically be enabled in production.
    /// </remarks>
    public bool UseHttpsRedirection { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to configure TLS options.
    /// </summary>
    public bool ConfigureTls { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use Client Certificate Mode from environment variables.
    /// </summary>
    public bool UseClientCertificateMode { get; set; } = true;
}
