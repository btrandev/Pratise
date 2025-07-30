# Common.Middleware Library

## Overview

This library provides a collection of reusable middleware components for ASP.NET Core applications, organized in a clean, maintainable structure. Each middleware component can be used independently or together using the unified configuration API.

## Installation

To install the package from your local feed:

```bash
dotnet add package YourCompany.Common.Middleware
```

## Structure

The library is organized into the following folders:

- **Authorization**: Authorization-related components and exceptions
  - `AuthorizationException.cs` - Custom exception for authorization failures

- **Behaviors**: MediatR pipeline behaviors for validation and logging
  - `LoggingBehavior.cs` - For logging requests and responses
  - `ValidationBehavior.cs` - For validating requests

- **Extensions**: Extension methods for configuring middleware components
  - `ApplicationExtensions.cs` - Main entry point for configuring all middleware
  - `AntiforgeryExtensions.cs` - For configuring antiforgery middleware
  - `CachingExtensions.cs` - For configuring caching middleware
  - `CorsExtensions.cs` - For configuring CORS middleware
  - `ExceptionHandlingExtensions.cs` - For configuring exception handling middleware
  - `HealthCheckExtensions.cs` - For configuring health checks middleware
  - `MetricsExtensions.cs` - For configuring metrics middleware
  - `RateLimitingExtensions.cs` - For configuring rate limiting middleware
  - `RequestLoggingExtensions.cs` - For configuring request logging middleware
  - `SecurityHeadersExtensions.cs` - For configuring security headers middleware

- **Health**: Health check middleware
  - Custom health check implementations

- **Logging**: Logging middleware
  - `RequestLoggingMiddleware.cs` - For logging HTTP requests

- **Middleware**: Core middleware implementations
  - `ExceptionHandlingMiddleware.cs` - For global exception handling

- **Metrics**: Metrics middleware
  - Custom metrics middleware implementations

- **Options**: Configuration options for middleware components
  - `ApplicationOptions.cs` - Main options for configuring all middleware
  - `AntiforgeryOptions.cs` - Options for antiforgery middleware
  - `CachingOptions.cs` - Options for caching middleware
  - `CorsOptions.cs` - Options for CORS middleware
  - `ExceptionHandlingOptions.cs` - Options for exception handling middleware
  - `HealthCheckOptions.cs` - Options for health checks middleware
  - `MetricsOptions.cs` - Options for metrics middleware
  - `RateLimitingOptions.cs` - Options for rate limiting middleware
  - `RequestLoggingOptions.cs` - Options for request logging middleware
  - `SecurityHeadersOptions.cs` - Options for security headers middleware

- **Performance**: Performance-related middleware
  - **Caching**: Caching middleware
    - `CachingMiddleware.cs` - For custom response caching

- **RateLimiting**: Rate limiting middleware
  - `IpRateLimitingMiddleware.cs` - For IP-based rate limiting

- **Security**: Security-related middleware
  - **Antiforgery**: Antiforgery middleware
    - `AntiforgeryMiddleware.cs` - For antiforgery token validation
  - `SecurityHeadersMiddleware.cs` - For adding security headers to responses

## Usage

### Basic Usage

To use all middleware components with default settings:

```csharp
// In Program.cs or Startup.cs

// Add services
builder.Services.AddAllMiddleware();

// Configure the middleware pipeline
app.UseAllMiddleware();
```

### Custom Configuration

To configure specific middleware components:

```csharp
// Add services with custom configuration
builder.Services.AddAllMiddleware(options =>
{
    options.UseRateLimiting = true;
    options.UseSecurityHeaders = true;
    options.UseCors = false;
    // ... other options
});

// Or configure individual components
builder.Services.AddSecurityHeaders(options =>
{
    options.AddContentSecurityPolicy = true;
    options.ContentSecurityPolicy = "default-src 'self';";
});

builder.Services.AddRateLimiting(options =>
{
    options.GlobalPermitLimit = 500;
    options.FixedWindowDurationSeconds = 30;
});

// Configure the middleware pipeline
app.UseAllMiddleware();
```

### Using Individual Components

Each middleware component can be used independently:

```csharp
// Add services
builder.Services.AddSecurityHeaders();
builder.Services.AddRateLimiting();
builder.Services.AddExceptionHandling();

// Configure middleware pipeline
app.UseSecurityHeaders();
app.UseRateLimiting();
app.UseExceptionHandling();
```

## Available Middleware Components

### Exception Handling Middleware

Provides global exception handling for ASP.NET Core applications. It converts exceptions to problem details responses with appropriate HTTP status codes.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
app.ConfigureExceptionHandler();
```

### Security Headers Middleware

Adds important security headers to HTTP responses to improve the security of your application.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
app.UseSecurityHeaders();
```

The following headers are added:

- X-Content-Type-Options
- X-Frame-Options
- X-XSS-Protection
- Referrer-Policy
- Strict-Transport-Security
- Content-Security-Policy

### CORS Middleware

Provides standardized CORS (Cross-Origin Resource Sharing) configurations.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddDefaultCorsPolicy();
// or with custom configuration
builder.Services.AddConfiguredCorsPolicy("MyPolicy", new[] { "https://example.com" }, true);

// Configure middleware
app.UseDefaultCorsPolicy();
// or with custom policy
app.UseConfiguredCorsPolicy("MyPolicy");
```

### Request Logging Middleware

Logs detailed information about HTTP requests, including duration and status code.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
app.UseRequestLogging();
```

### Metrics Middleware

Adds Prometheus metrics collection and exposes metrics endpoints.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
app.UsePrometheusMetrics();
// or with custom configuration
app.UseConfigurablePrometheusMetrics("/metrics", true, true);
```

### Health Check Middleware

Adds customizable health check endpoints for monitoring application health.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
app.MapHealthChecks();
// or with custom paths
app.MapHealthChecks("/api/health", "/api/health/details");
// Add specific endpoints for Kubernetes
app.MapLiveEndpoint("/health/live");
app.MapReadyEndpoint("/health/ready");
```

### Response Compression Middleware

Adds response compression to reduce payload size.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddDefaultResponseCompression();
// or with high compression
builder.Services.AddHighCompressionResponse();

// Configure middleware
app.UseResponseCompression();
```

### Standard Middleware Configuration

Configure all middleware components at once with sensible defaults.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddStandardMiddlewareServices();

// Configure middleware
app.UseStandardMiddleware();
// or with custom options
app.UseStandardMiddleware(
    useCors: true,
    useCompression: true,
    useSecurityHeaders: true,
    useRequestLogging: true,
    useMetrics: true
);
```

## MediatR Behaviors

The package includes reusable MediatR pipeline behaviors for common cross-cutting concerns.

### ValidationBehavior

Validates requests using FluentValidation before they reach the handlers. This behavior will:

- Automatically find and execute all IValidator<TRequest> implementations for a given request
- Collect all validation errors across multiple validators
- Throw a ValidationException if any validation errors are found
- Continue to the next handler if all validations pass

### LoggingBehavior

Logs the handling of requests and their execution time. This behavior will:

- Log when a request starts processing
- Measure the execution time of the request handler
- Log when a request completes processing with the total execution time
- Log exceptions that occur during request handling

### Usage

```csharp
// Add the using statement
using Common.Middleware.Behaviors;

// Register all behaviors at once
builder.Services.AddStandardMediatRBehaviors();

// Or with MediatR in one call
builder.Services.AddMediatRWithBehaviors(Assembly.GetExecutingAssembly());
```

The `AddMediatRWithBehaviors` extension method accepts either a single assembly or multiple assemblies where your MediatR handlers are located.

## Authorization

The package also includes authorization-related components:

### AuthorizationException

An exception that can be thrown when a user doesn't have the required permissions to perform an action.

Usage:

```csharp
using Common.Middleware.Authorization;

// Throw the exception when a user doesn't have permission
throw new AuthorizationException("You do not have permission to perform this action");
```

### Response Caching Middleware

Adds HTTP response caching to improve performance for frequently requested resources.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddDefaultResponseCaching();

// Configure middleware
app.UseDefaultResponseCaching();
```

### Rate Limiting Middleware

Limits the rate of client requests to protect your application from abuse.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddDefaultRateLimiter();

// Configure middleware
app.UseDefaultRateLimiting();
```

### Antiforgery Middleware

Protects against Cross-Site Request Forgery (CSRF) attacks.

Usage:

```csharp
// Add the using statement
using Common.Middleware.Infrastructure;

// In your Program.cs or Startup.cs
// Add services
builder.Services.AddDefaultAntiforgery();

// Configure middleware
var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
app.UseAntiforgeryValidation(antiforgery);
app.UseAntiforgeryTokens(antiforgery);
```

## Development

To build the package locally:

```bash
dotnet build
```

To pack the library into a NuGet package:

```bash
dotnet pack -c Release
```
