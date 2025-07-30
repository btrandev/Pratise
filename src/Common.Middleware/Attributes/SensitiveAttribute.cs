using System;

namespace Common.Middleware.Attributes;

/// <summary>
/// Marks a property as containing sensitive information that should be redacted in logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SensitiveAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveAttribute"/> class.
    /// </summary>
    public SensitiveAttribute()
    {
    }
}
