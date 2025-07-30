namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring exception handling middleware
/// </summary>
public class ExceptionHandlingOptions
{
    /// <summary>
    /// Gets or sets whether to include exception details in the response
    /// </summary>
    public bool IncludeExceptionDetails { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to log the exception
    /// </summary>
    public bool LogExceptions { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the default error message when details are hidden
    /// </summary>
    public string DefaultErrorMessage { get; set; } = "An unexpected error occurred";
    
    /// <summary>
    /// Gets or sets whether to use ProblemDetails format
    /// </summary>
    public bool UseProblemDetails { get; set; } = true;
}
