using System;

namespace Common.Middleware.Authorization;

/// <summary>
/// Exception thrown when a user does not have the required permissions for an operation
/// </summary>
public class AuthorizationException : Exception
{
    public AuthorizationException() : base("You do not have permission to perform this operation")
    {
    }

    public AuthorizationException(string message) : base(message)
    {
    }

    public AuthorizationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
