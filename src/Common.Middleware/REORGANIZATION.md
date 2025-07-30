# Common.Middleware Reorganization Guide

## Overview

This document provides guidance on the reorganization of the Common.Middleware library from a flat structure with an Infrastructure folder to a more organized, domain-specific folder structure.

## Directory Structure

The new structure is organized as follows:

```
Common.Middleware/
├── Authorization/         # Authorization-related components
├── Behaviors/             # MediatR pipeline behaviors
├── Extensions/            # Extension methods for middleware configuration
├── Health/                # Health check middleware
├── Logging/               # Logging middleware
├── Middleware/            # Core middleware implementations
├── Metrics/               # Metrics middleware
├── Options/               # Configuration options for all middleware
├── Performance/           # Performance-related middleware
│   └── Caching/           #   Caching middleware
├── RateLimiting/          # Rate limiting middleware
├── Security/              # Security-related middleware
│   └── Antiforgery/       #   Antiforgery middleware
└── Infrastructure/        # OLD folder structure (to be removed after migration)
```

## Migration Guide

### Step 1: Create New Option Classes

Create option classes for each middleware component in the Options folder:

```csharp
// Example: Options/SecurityHeadersOptions.cs
namespace Common.Middleware.Options;

public class SecurityHeadersOptions 
{
    public bool AddContentSecurityPolicy { get; set; } = true;
    // Other properties...
}
```

### Step 2: Create Extension Methods

Create extension methods for each middleware component in the Extensions folder:

```csharp
// Example: Extensions/SecurityHeadersExtensions.cs
namespace Common.Middleware.Extensions;

public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddSecurityHeaders(
        this IServiceCollection services,
        Action<SecurityHeadersOptions>? configureOptions = null)
    {
        // Implementation...
    }
    
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app)
    {
        // Implementation...
    }
}
```

### Step 3: Move Middleware Implementations

Move middleware implementations to their respective domain folders:

```csharp
// Example: Security/SecurityHeadersMiddleware.cs
namespace Common.Middleware.Security;

public class SecurityHeadersMiddleware
{
    // Implementation...
}
```

### Step 4: Update References

Update references in the code to point to the new namespaces:

```csharp
// Before
using Common.Middleware.Infrastructure;

// After
using Common.Middleware.Extensions;
using Common.Middleware.Security;
```

### Step 5: Update ApplicationExtensions.cs

Update the main entry point to use the new extension methods:

```csharp
// Example in ApplicationExtensions.cs
if (options.UseSecurityHeaders)
{
    // Before
    app.UseDefaultSecurityHeaders();
    
    // After
    app.UseSecurityHeaders();
}
```

## Migration Status

### Completed
- [x] Created folder structure
- [x] Created Options classes
- [x] Created Extension methods
- [x] Created some middleware implementations
- [x] Updated README.md

### Pending
- [ ] Implement missing middleware classes
- [ ] Fix compile errors in ApplicationExtensions.cs
- [ ] Delete redundant Infrastructure folder
- [ ] Update unit tests

## Common Issues

1. **Compile Errors**: Most compile errors will be due to missing middleware implementations or incorrect method references.

2. **Namespace Conflicts**: Make sure all references point to the new namespaces, not the old Infrastructure namespace.

3. **Duplicate Code**: Delete old implementations in the Infrastructure folder once the migration is complete.

## Next Steps

1. Implement any missing middleware classes
2. Fix any compile errors
3. Run tests to ensure everything works as expected
4. Delete the Infrastructure folder once the migration is complete
