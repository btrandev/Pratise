using AdminService.Common.Authorization;
using Common.Middleware.Authorization;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Common.Authorization;

public class PermissionRequirementBehaviorWithAuthTests
{
    private readonly Mock<ILogger<PermissionRequirementBehavior<TestRequestWithAuth, string>>> _loggerMock;
    private readonly TestCurrentUser _currentUser;
    private readonly PermissionRequirementBehavior<TestRequestWithAuth, string> _behavior;
    
    public PermissionRequirementBehaviorWithAuthTests()
    {
        _loggerMock = new Mock<ILogger<PermissionRequirementBehavior<TestRequestWithAuth, string>>>();
        _currentUser = new TestCurrentUser(
            id: Guid.NewGuid(),
            username: "testuser",
            tenantId: Guid.NewGuid(),
            role: "User",
            isAuthenticated: true
        );
        
        _behavior = new PermissionRequirementBehavior<TestRequestWithAuth, string>(_currentUser, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenRequestRequiresAuthorizationAndUserIsNotAuthenticated_ShouldThrowException()
    {
        // Arrange
        var unauthenticatedUser = new TestCurrentUser(isAuthenticated: false);
        var behavior = new PermissionRequirementBehavior<TestRequestWithAuth, string>(
            unauthenticatedUser, 
            _loggerMock.Object
        );
        
        var request = new TestRequestWithAuth();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");
        
        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() => 
            behavior.Handle(request, next, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenRequestRequiresPermissionAndUserHasPermission_ShouldContinue()
    {
        // Arrange
        _currentUser.AddPermission("Test.Permission");
        
        var request = new TestRequestWithAuth();
        var expectedResult = "Success";
        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
        
        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
    }
    
    [Fact]
    public async Task Handle_WhenRequestRequiresPermissionAndUserDoesNotHavePermission_ShouldThrowException()
    {
        // Arrange
        // Not adding the required permission
        
        var request = new TestRequestWithAuth();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");
        
        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() => 
            _behavior.Handle(request, next, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenRequestRequiresPermissionAndUserIsAdmin_ShouldContinue()
    {
        // Arrange
        var adminUser = new TestCurrentUser(
            id: Guid.NewGuid(),
            username: "admin",
            tenantId: Guid.NewGuid(),
            role: "Admin",
            isAuthenticated: true
        );
        
        var behavior = new PermissionRequirementBehavior<TestRequestWithAuth, string>(
            adminUser, 
            _loggerMock.Object
        );
        
        var request = new TestRequestWithAuth();
        var expectedResult = "Success";
        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
        
        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
    }
    
    // Test class
    public class TestRequestWithAuth : IRequest<string>, IRequireAuthorization
    {
        public string[] RequiredPermissions => new[] { "Test.Permission" };
    }
}
