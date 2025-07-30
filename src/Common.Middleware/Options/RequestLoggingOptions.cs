namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring the request logging middleware
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Gets or sets whether to log successful requests. Default is true.
    /// </summary>
    public bool LogSuccessfulRequests { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the minimum threshold in milliseconds for logging slow requests. Default is 500ms.
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 500;
}
