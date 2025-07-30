namespace Common.Middleware.Results;

/// <summary>
/// Represents an error with a message and optional error code.
/// </summary>
public record Error
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public Error(string message)
        : this(string.Empty, message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
    
    /// <summary>
    /// Implicitly converts a string to an <see cref="Error"/> with the string as the message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static implicit operator Error(string message) => new(message);
    
    /// <summary>
    /// Returns the error message.
    /// </summary>
    /// <returns>The error message.</returns>
    public override string ToString() => string.IsNullOrEmpty(Code) ? Message : $"[{Code}] {Message}";
}
