using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Common.Middleware.Security;

/// <summary>
/// Middleware for adding security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="options">Options for configuring security headers</param>
    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions options)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    /// <summary>
    /// Processes an HTTP request by adding security headers to the response
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        var headers = context.Response.Headers;

        // Prevents MIME type sniffing
        if (_options.UseContentTypeOptions && !headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Add("X-Content-Type-Options", _options.ContentTypeOptions);
        }

        // Prevents the browser from rendering the page inside a frame/iframe
        if (_options.UseFrameOptions && !headers.ContainsKey("X-Frame-Options"))
        {
            headers.Add("X-Frame-Options", _options.XFrameOptions);
        }

        // Enables XSS protection in browsers
        if (_options.UseXssProtection && !headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Add("X-XSS-Protection", _options.XssProtection);
        }

        // Controls how much information the browser includes with referrers
        if (_options.UseReferrerPolicy && !headers.ContainsKey("Referrer-Policy"))
        {
            headers.Add("Referrer-Policy", _options.ReferrerPolicy);
        }

        // Helps protect against some types of cryptographic attacks
        if (_options.UseHsts && !headers.ContainsKey("Strict-Transport-Security"))
        {
            headers.Add("Strict-Transport-Security", _options.Hsts);
        }

        // Controls resources the user agent can load
        if (_options.UseContentSecurityPolicy && !headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Add("Content-Security-Policy", _options.ContentSecurityPolicy);
        }

        await _next(context);
    }
}
