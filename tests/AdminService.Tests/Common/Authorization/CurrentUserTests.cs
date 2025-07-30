using System.Security.Claims;
using AdminService.Common.Authorization;
using Common.Middleware.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AdminService.Tests.Common.Authorization;

public class CurrentUserTests
{
    [Fact]
    public void Id_WhenUserHasNameIdentifierClaim_ShouldReturnId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var id = currentUser.Id;
        
        // Assert
        id.Should().Be(userId);
    }
    
    [Fact]
    public void Id_WhenUserDoesNotHaveNameIdentifierClaim_ShouldReturnNull()
    {
        // Arrange
        var httpContextMock = CreateMockHttpContextWithClaims(Array.Empty<Claim>());
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var id = currentUser.Id;
        
        // Assert
        id.Should().BeNull();
    }
    
    [Fact]
    public void Username_WhenUserHasNameClaim_ShouldReturnUsername()
    {
        // Arrange
        var username = "testuser";
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim(ClaimTypes.Name, username)
        });
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.Username;
        
        // Assert
        result.Should().Be(username);
    }
    
    [Fact]
    public void TenantId_WhenUserHasTenantIdClaim_ShouldReturnTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim("tenantId", tenantId.ToString())
        });
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.TenantId;
        
        // Assert
        result.Should().Be(tenantId);
    }
    
    [Fact]
    public void Role_WhenUserHasRoleClaim_ShouldReturnRole()
    {
        // Arrange
        var role = "Admin";
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim(ClaimTypes.Role, role)
        });
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.Role;
        
        // Assert
        result.Should().Be(role);
    }
    
    [Fact]
    public void HasPermission_WhenUserIsAdmin_ShouldReturnTrue()
    {
        // Arrange
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, true);
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.HasPermissions("AnyPermission");
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void HasPermission_WhenUserHasPermissionClaim_ShouldReturnTrue()
    {
        // Arrange
        var permission = "Users.View";
        var httpContextMock = CreateMockHttpContextWithClaims(new[] 
        {
            new Claim("permission", permission)
        }, true);
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.HasPermissions(permission);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void HasPermission_WhenUserDoesNotHavePermissionClaim_ShouldReturnFalse()
    {
        // Arrange
        var httpContextMock = CreateMockHttpContextWithClaims(Array.Empty<Claim>(), true);
        
        var currentUser = new CurrentUser(httpContextMock.Object);
        
        // Act
        var result = currentUser.HasPermissions("Users.View");
        
        // Assert
        result.Should().BeFalse();
    }
    
    private Mock<IHttpContextAccessor> CreateMockHttpContextWithClaims(IEnumerable<Claim> claims, bool isAuthenticated = false)
    {
        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        
        var user = new Mock<ClaimsPrincipal>();
        user.Setup(x => x.Claims).Returns(claims);
        user.Setup(x => x.Identity).Returns(identity.Object);
        
        foreach (var claim in claims)
        {
            user.Setup(x => x.HasClaim(claim.Type, claim.Value)).Returns(true);
        }
        
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.User).Returns(user.Object);
        
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
        
        return httpContextAccessor;
    }
}
