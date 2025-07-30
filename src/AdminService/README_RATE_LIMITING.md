# Rate Limiting

This document describes how to implement rate limiting in the AdminService API.

## Overview

The AdminService uses ASP.NET Core's built-in rate limiting middleware to control the frequency of requests to specific endpoints. Rate limiting helps prevent abuse, brute force attacks, and ensures fair usage of resources.

## Available Rate Limiters

The application includes several preconfigured rate limiters:

1. **Fixed Window Limiter** (`"fixed"`) - Limits requests in a fixed time window
2. **Sliding Window Limiter** (`"sliding"`) - Limits requests using a sliding time window
3. **Token Bucket Limiter** (`"token"`) - Uses a token bucket algorithm for rate limiting
4. **Global Limiter** - Applied to all endpoints if enabled
5. **IP-Based Rate Limiters**:
   - `"ip-rate-limit-per-second"` - Limits to 1 request per second per IP address
   - `"ip-rate-limit-per-minute"` - Limits to 5 requests per minute per IP address
   - `"ip-rate-limit-per-hour"` - Limits to 30 requests per hour per IP address

## Applying Rate Limiting

### IP-Based Rate Limiting Options

To limit an endpoint based on client IP address, use one of the following extension methods:

```csharp
// Add the using statement
using Common.Middleware.Extensions;

// In your endpoint mapping method - choose one of the following:

// Limit to 1 request per second per IP address
endpoints.MapPost("/api/resources", async (context) => { /* handler code */ })
    .RequireRateLimitingPerSecond();

// Limit to 5 requests per minute per IP address
endpoints.MapPost("/api/resources", async (context) => { /* handler code */ })
    .RequireRateLimitingByIpPerMinute();

// Limit to 30 requests per hour per IP address
endpoints.MapPost("/api/resources", async (context) => { /* handler code */ })
    .RequireRateLimitingByIpPerHour();
```

### Example Endpoints with Rate Limiting

The following endpoints are currently rate limited by IP address:

1. `/api/auth/login` - Limited to 1 request per second per IP to prevent brute force login attempts
2. `/api/users/{userId}/permissions` - Limited to 1 request per second per IP to prevent permission tampering

## Configuration

Rate limiting options can be configured in `appsettings.json`:

```json
{
  "RateLimiting": {
    "EnableFixedWindowLimiter": true,
    "FixedWindowPermitLimit": 100,
    "FixedWindowDurationSeconds": 60,
    "EnableGlobalLimiter": false
  }
}
```

Or programmatically in `Program.cs`:

```csharp
builder.Services.AddRateLimiting(options => {
    options.EnableFixedWindowLimiter = true;
    options.FixedWindowPermitLimit = 100;
    options.FixedWindowDurationSeconds = 60;
});
```

## Testing Rate Limits

You can test the rate limits using a tool like curl:

```bash
# Send multiple requests quickly to trigger the rate limit
for i in {1..10}; do curl -i -H "Content-Type: application/json" -X POST -d '{"username":"test","password":"test"}' http://localhost:5000/api/auth/login; done
```

After the first request, you should receive a 429 (Too Many Requests) response.
