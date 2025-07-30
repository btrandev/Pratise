using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace AdminService.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "AdminService")
            .WriteTo.Console()
            .WriteTo.File("logs/admin-service-.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.GrafanaLoki(
                builder.Configuration.GetValue<string>("Serilog:Loki:Uri") ?? "http://loki:3100",
                labels: new[] { 
                    new LokiLabel { Key = "app", Value = "adminservice" }, 
                    new LokiLabel { Key = "env", Value = builder.Environment.EnvironmentName } 
                })
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}