using AdminService.Common.Authorization;
using Common.Middleware.Authorization;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Common.Authorization;

public class PermissionRequirementBehaviorTests
{
    private readonly Mock<ILogger<PermissionRequirementBehavior<TestRequest, string>>> _loggerMock;
    private readonly TestCurrentUser _currentUser;
    private readonly PermissionRequirementBehavior<TestRequest, string> _behavior;
    
    public PermissionRequirementBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<PermissionRequirementBehavior<TestRequest, string>>>();
        _currentUser = new TestCurrentUser(
            id: Guid.NewGuid(),
            username: "testuser",
            tenantId: Guid.NewGuid(),
            role: "User",
            isAuthenticated: true
        );
        
        _behavior = new PermissionRequirementBehavior<TestRequest, string>(_currentUser, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WhenRequestDoesNotRequireAuthorization_ShouldContinue()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResult = "Success";
        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
        
        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
    }
    
    // Test class for requests that don't require authorization
    public class TestRequest : IRequest<string>
    {
    }
}
