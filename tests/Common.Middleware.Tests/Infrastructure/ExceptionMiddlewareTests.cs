using Common.Middleware.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Common.Middleware.Tests.Infrastructure;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task ConfigureExceptionHandler_ShouldCatchAndReturnAppropriateResponse()
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
                        app.ConfigureExceptionHandler();
                        app.Run(context => throw new ArgumentException("Test exception"));
                    });
            })
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        content.Should().Contain("Test exception");
        content.Should().Contain("ArgumentException");
    }

    [Fact]
    public async Task ConfigureExceptionHandler_WithUnhandledException_ShouldReturn500()
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
                        app.ConfigureExceptionHandler();
                        app.Run(context => throw new Exception("Unexpected error"));
                    });
            })
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        content.Should().Contain("An unexpected error occurred");
        
        // In production mode, we wouldn't expose the actual exception message
        // but in development mode, we should see it
        content.Should().Contain("Unexpected error");
    }

    [Fact]
    public async Task ConfigureExceptionHandler_WithoutException_ShouldPassThrough()
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
                        app.ConfigureExceptionHandler();
                        app.Run(context => 
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            return context.Response.WriteAsync("Success");
                        });
                    });
            })
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Success");
    }
}
