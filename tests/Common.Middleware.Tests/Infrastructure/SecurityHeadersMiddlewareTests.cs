using Common.Middleware.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Common.Middleware.Tests.Infrastructure;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task UseSecurityHeaders_ShouldAddSecurityHeadersToResponse()
    {
        // Arrange
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => { })
                    .Configure(app =>
                    {
                        app.UseSecurityHeaders();
                        app.Run(context => context.Response.WriteAsync("Hello, world!"));
                    });
            })
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/");
        
        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.Should().ContainKey("Content-Security-Policy");
        
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");
        response.Headers.GetValues("X-XSS-Protection").First().Should().Be("1; mode=block");
        response.Headers.GetValues("Referrer-Policy").First().Should().Be("strict-origin-when-cross-origin");
    }
    
    [Fact]
    public async Task UseSecurityHeaders_WithCustomOptions_ShouldAddCustomHeadersToResponse()
    {
        // Arrange
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => { })
                    .Configure(app =>
                    {
                        app.UseSecurityHeaders(new SecurityHeadersOptions
                        {
                            XFrameOptions = "SAMEORIGIN",
                            ReferrerPolicy = "no-referrer"
                        });
                        app.Run(context => context.Response.WriteAsync("Hello, world!"));
                    });
            })
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/");
        
        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("Referrer-Policy");
        
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("SAMEORIGIN");
        response.Headers.GetValues("Referrer-Policy").First().Should().Be("no-referrer");
    }
}
