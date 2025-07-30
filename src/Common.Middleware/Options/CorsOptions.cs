namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring CORS
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// Gets or sets whether to allow any origin. Default is true.
    /// </summary>
    public bool AllowAnyOrigin { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to allow any header. Default is true.
    /// </summary>
    public bool AllowAnyHeader { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to allow credentials. Default is true.
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the allowed origins if AllowAnyOrigin is false.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the allowed headers if AllowAnyHeader is false.
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the exposed headers.
    /// </summary>
    public List<string> ExposedHeaders { get; set; } = new List<string>();
}
