using AdminService.Domain.Entities;
using AdminService.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace AdminService.Tests.Infrastructure.Authentication;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtService _jwtService;
    private readonly string _secret = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123!@#";
    private readonly string _issuer = "test-issuer";
    private readonly string _audience = "test-audience";
    private readonly int _expiryMinutes = 60;
    
    public JwtServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x["Secret"]).Returns(_secret);
        jwtSection.Setup(x => x["Issuer"]).Returns(_issuer);
        jwtSection.Setup(x => x["Audience"]).Returns(_audience);
        jwtSection.Setup(x => x["ExpiryMinutes"]).Returns(_expiryMinutes.ToString());
        jwtSection.Setup(x => x["RefreshExpiryDays"]).Returns("7");
        
        _configurationMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
        
        _jwtService = new JwtService(_configurationMock.Object);
    }
    
    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser();
        
        // Act
        var token = _jwtService.GenerateAccessToken(user);
        
        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Verify token can be validated
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters();
        
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        
        principal.Should().NotBeNull();
        validatedToken.Should().NotBeNull();
        
        // Check claims
        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.Should().Be(user.Email);
        principal.FindFirst("tenant_id")?.Value.Should().Be(user.TenantId.ToString());
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be(user.Role);
    }
    
    [Fact]
    public void GenerateRefreshToken_ShouldReturnGuid()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        Guid.TryParse(refreshToken, out _).Should().BeTrue();
    }
    
    [Fact]
    public void GenerateLoginResponse_ShouldReturnValidResponse()
    {
        // Arrange
        var user = CreateTestUser();
        
        // Act
        var response = _jwtService.GenerateLoginResponse(user);
        
        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        
        response.User.Should().NotBeNull();
        response.User.Id.Should().Be(user.Id);
        response.User.Email.Should().Be(user.Email);
        response.User.FirstName.Should().Be(user.FirstName);
        response.User.LastName.Should().Be(user.LastName);
        response.User.Role.Should().Be(user.Role);
        response.User.TenantId.Should().Be(user.TenantId);
    }
    
    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _jwtService.GenerateAccessToken(user);
        
        // Act
        var principal = _jwtService.ValidateToken(token);
        
        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
    }
    
    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";
        
        // Act
        var principal = _jwtService.ValidateToken(invalidToken);
        
        // Assert
        principal.Should().BeNull();
    }
    
    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(-10), // Expired token
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var expiredToken = tokenHandler.WriteToken(token);
        
        // Act
        var principal = _jwtService.ValidateToken(expiredToken);
        
        // Assert
        principal.Should().BeNull();
    }
    
    private User CreateTestUser()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Username = "testuser",
            Role = "Admin",
            TenantId = tenantId,
            Claims = new List<UserClaim>
            {
                new UserClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = "Users.View"
                },
                new UserClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = "Tenants.View"
                }
            }
        };
        
        return user;
    }
    
    private TokenValidationParameters GetValidationParameters()
    {
        var key = Encoding.ASCII.GetBytes(_secret);
        
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
