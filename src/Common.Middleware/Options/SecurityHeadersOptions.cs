namespace Common.Middleware.Security;

/// <summary>
/// Options for configuring security headers middleware
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Gets or sets whether to use Content-Type-Options header
    /// </summary>
    public bool UseContentTypeOptions { get; set; } = true;

    /// <summary>
    /// Gets or sets the Content-Type-Options header value
    /// </summary>
    public string ContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// Gets or sets whether to use X-Frame-Options header
    /// </summary>
    public bool UseFrameOptions { get; set; } = true;

    /// <summary>
    /// Gets or sets the X-Frame-Options header value
    /// </summary>
    public string XFrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Gets or sets whether to use X-XSS-Protection header
    /// </summary>
    public bool UseXssProtection { get; set; } = true;

    /// <summary>
    /// Gets or sets the X-XSS-Protection header value
    /// </summary>
    public string XssProtection { get; set; } = "1; mode=block";

    /// <summary>
    /// Gets or sets whether to use Referrer-Policy header
    /// </summary>
    public bool UseReferrerPolicy { get; set; } = true;

    /// <summary>
    /// Gets or sets the Referrer-Policy header value
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Gets or sets whether to use Strict-Transport-Security header
    /// </summary>
    public bool UseHsts { get; set; } = true;

    /// <summary>
    /// Gets or sets the Strict-Transport-Security header value
    /// </summary>
    public string Hsts { get; set; } = "max-age=31536000; includeSubDomains";

    /// <summary>
    /// Gets or sets whether to use Content-Security-Policy header
    /// </summary>
    public bool UseContentSecurityPolicy { get; set; } = true;

    /// <summary>
    /// Gets or sets the Content-Security-Policy header value
    /// </summary>
    public string ContentSecurityPolicy { get; set; } = "default-src 'self'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "connect-src 'self'";
}
