using Common.Middleware.Extensions;

namespace AdminService.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void ConfigureMiddlewarePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use standard middleware from Common.Middleware package
        app.UseStandardMiddleware();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map health check endpoint
        app.MapHealthChecks("/health");
    }
}