using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdminService.Domain.Entities;
using AdminService.Features.Auth.Login;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdminService.Infrastructure.Authentication;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    LoginResponse GenerateLoginResponse(User user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;
    private readonly int _refreshExpiryDays;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        var jwtSettings = _configuration.GetSection("Jwt");
        _secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
        _refreshExpiryDays = int.Parse(jwtSettings["RefreshExpiryDays"] ?? "7");
    }

    public LoginResponse GenerateLoginResponse(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var expiryTime = DateTime.UtcNow.AddMinutes(_expiryMinutes);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiryTime,
            User = new LoginResponse.UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                TenantId = user.TenantId
            }
        };
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("tenant_id", user.TenantId.ToString())
        };

        // Add role claim if present
        if (!string.IsNullOrEmpty(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        // Add custom claims
        foreach (var claim in user.Claims)
        {
            claims.Add(new Claim(claim.ClaimType, claim.ClaimValue));
            
            // For permission claims, add them with a specific type for easier access in authorization checks
            if (claim.ClaimType == "permission")
            {
                claims.Add(new Claim("permissions", claim.ClaimValue));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return principal;
        }
        catch
        {
            // Return null if token validation fails
            return null;
        }
    }
}
