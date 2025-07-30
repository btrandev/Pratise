using System.Text;
using AdminService.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace AdminService.Infrastructure.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Authentication and Authorization
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<Authorization.PermissionManager>();

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        // Use a default secret for development if none is configured
        var secretKey = jwtSettings["Secret"] ?? "YourSuperSecretKeyWithAtLeast32Characters!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Log JWT configuration status
        if (jwtSettings["Secret"] == null)
        {
            Log.Warning("JWT Secret not found in configuration. Using default development secret. DO NOT USE IN PRODUCTION!");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "AdminService",
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"] ?? "AdminServiceClients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}