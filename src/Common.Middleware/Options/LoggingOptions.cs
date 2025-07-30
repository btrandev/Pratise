namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring logging behavior
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether request payloads should be logged.
    /// This should typically be disabled in production environments.
    /// </summary>
    public bool LogPayloads { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the maximum payload size (in characters) to log.
    /// Payloads larger than this will be truncated.
    /// </summary>
    public int MaxPayloadSize { get; set; } = 10000;
}
