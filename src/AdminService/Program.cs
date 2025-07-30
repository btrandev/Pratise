using AdminService.Infrastructure.Data;
using AdminService.Infrastructure.Extensions;
using Common.Middleware.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.ConfigureSerilog();

// Add logger factory to check if FluentValidation is working properly
builder.Logging.AddFilter("FluentValidation", LogLevel.Debug);
builder.Logging.AddFilter("Common.Middleware.Behaviors", LogLevel.Debug);

// Register all services
builder.Services
    .AddApiServices()
    .AddStandardMiddlewareServices()
    .AddValidationServices()
    .AddInfrastructureServices(builder.Configuration, builder.Environment.EnvironmentName)
    .AddAuthenticationServices(builder.Configuration)
    .AddMediatrServices()
    .AddRepositories()
    .AddHealthChecks()
    .AddDbContextCheck<AdminServiceDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

// Build the application
var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddlewarePipeline();

// Map API endpoints
app.MapApplicationEndpoints();

// Run the application
try
{
    Log.Information("Starting Admin Service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration testing
public partial class Program { }