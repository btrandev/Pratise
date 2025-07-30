using AdminService.Common.Authorization;
using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Authorization;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdminService.Tests.Infrastructure.Authorization;

public class PermissionManagerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly PermissionManager _permissionManager;
    
    public PermissionManagerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _permissionManager = new PermissionManager(_userRepositoryMock.Object);
    }
    
    [Fact]
    public async Task SyncPermissionsForUserAsync_WhenUserIsAdmin_ShouldSetAdminPermissions()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Role = "Admin",
            Claims = new List<UserClaim>()
        };
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.SyncPermissionsForUserAsync(user);
        
        // Assert
        user.Claims.Should().HaveCount(Permissions.Roles.Admin.Length);
        user.Claims.All(c => c.ClaimType == "permission").Should().BeTrue();
        
        foreach (var permission in Permissions.Roles.Admin)
        {
            user.Claims.Any(c => c.ClaimValue == permission).Should().BeTrue();
        }
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task SyncPermissionsForUserAsync_WhenUserIsTenantAdmin_ShouldSetTenantAdminPermissions()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Role = "TenantAdmin",
            Claims = new List<UserClaim>()
        };
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.SyncPermissionsForUserAsync(user);
        
        // Assert
        user.Claims.Should().HaveCount(Permissions.Roles.TenantAdmin.Length);
        user.Claims.All(c => c.ClaimType == "permission").Should().BeTrue();
        
        foreach (var permission in Permissions.Roles.TenantAdmin)
        {
            user.Claims.Any(c => c.ClaimValue == permission).Should().BeTrue();
        }
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task SyncPermissionsForUserAsync_WhenUserIsStandardUser_ShouldSetStandardUserPermissions()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Role = "User", // Standard user role
            Claims = new List<UserClaim>()
        };
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.SyncPermissionsForUserAsync(user);
        
        // Assert
        user.Claims.Should().HaveCount(Permissions.Roles.StandardUser.Length);
        user.Claims.All(c => c.ClaimType == "permission").Should().BeTrue();
        
        foreach (var permission in Permissions.Roles.StandardUser)
        {
            user.Claims.Any(c => c.ClaimValue == permission).Should().BeTrue();
        }
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task SyncPermissionsForUserAsync_ShouldRemoveExistingPermissionsBeforeAddingNewOnes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Role = "Admin",
            Claims = new List<UserClaim>
            {
                new UserClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = "OldPermission"
                }
            }
        };
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.SyncPermissionsForUserAsync(user);
        
        // Assert
        user.Claims.Should().HaveCount(Permissions.Roles.Admin.Length);
        user.Claims.Any(c => c.ClaimValue == "OldPermission").Should().BeFalse();
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task AddPermissionToUserAsync_WhenPermissionDoesNotExist_ShouldAddPermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            CreatedById = Guid.NewGuid(),
            CreatedByName = "TestUser",
            Claims = new List<UserClaim>()
        };
        
        var permission = "Users.View";
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.AddPermissionToUserAsync(user, permission);
        
        // Assert
        user.Claims.Should().HaveCount(1);
        user.Claims.First().ClaimType.Should().Be("permission");
        user.Claims.First().ClaimValue.Should().Be(permission);
        user.Claims.First().UserId.Should().Be(userId);
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task AddPermissionToUserAsync_WhenPermissionAlreadyExists_ShouldNotAddDuplicate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permission = "Users.View";
        
        var user = new User
        {
            Id = userId,
            Claims = new List<UserClaim>
            {
                new UserClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = permission
                }
            }
        };
        
        // Act
        await _permissionManager.AddPermissionToUserAsync(user, permission);
        
        // Assert
        user.Claims.Should().HaveCount(1);
        
        // Verify repository was not called
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Fact]
    public async Task RemovePermissionFromUserAsync_WhenPermissionExists_ShouldRemovePermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permission = "Users.View";
        
        var user = new User
        {
            Id = userId,
            Claims = new List<UserClaim>
            {
                new UserClaim
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ClaimType = "permission",
                    ClaimValue = permission
                }
            }
        };
        
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        
        // Act
        await _permissionManager.RemovePermissionFromUserAsync(user, permission);
        
        // Assert
        user.Claims.Should().BeEmpty();
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
    
    [Fact]
    public async Task RemovePermissionFromUserAsync_WhenPermissionDoesNotExist_ShouldNotCallRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Claims = new List<UserClaim>()
        };
        
        // Act
        await _permissionManager.RemovePermissionFromUserAsync(user, "Users.View");
        
        // Assert
        user.Claims.Should().BeEmpty();
        
        // Verify repository was not called
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
}
