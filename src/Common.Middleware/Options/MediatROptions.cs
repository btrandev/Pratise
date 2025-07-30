namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring MediatR behaviors
/// </summary>
public class MediatROptions
{
    /// <summary>
    /// Gets or sets whether to use validation behavior
    /// </summary>
    public bool UseValidationBehavior { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use logging behavior
    /// </summary>
    public bool UseLoggingBehavior { get; set; } = true;
}
