using Common.Middleware.Behaviors;
using Common.Middleware.Options;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using Xunit;

namespace AdminService.Tests.Common.Behaviors;
public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, string>>> _loggerMock;
    private readonly Mock<IOptions<LoggingOptions>> _loggingOptionsMock;
    private readonly LoggingBehavior<TestRequest, string> _behavior;
    
    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        _loggingOptionsMock = new Mock<IOptions<LoggingOptions>>();
        _loggingOptionsMock.Setup(o => o.Value).Returns(new LoggingOptions());
        _behavior = new LoggingBehavior<TestRequest, string>(_loggerMock.Object, _loggingOptionsMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldLogStartAndEnd()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResult = "Success";
        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
        
        // Configure logger to capture log statements
        bool startLogCaptured = false;
        bool endLogCaptured = false;
        
        _loggerMock.Setup(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Starting request {typeof(TestRequest).Name}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(() => startLogCaptured = true);
        
        _loggerMock.Setup(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Completed request {typeof(TestRequest).Name}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(() => endLogCaptured = true);
        
        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
        startLogCaptured.Should().BeTrue();
        endLogCaptured.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");
        RequestHandlerDelegate<string> next = () => throw expectedException;
        
        // Configure logger to capture error log statements
        bool errorLogCaptured = false;
        
        _loggerMock.Setup(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(() => errorLogCaptured = true);
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _behavior.Handle(request, next, CancellationToken.None));
        
        errorLogCaptured.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_ShouldMeasureExecutionTime()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResult = "Success";
        
        // Simulate a delay in the next delegate to ensure execution time is measured
        RequestHandlerDelegate<string> next = async () => 
        {
            await Task.Delay(100); // Small delay to ensure measurable time
            return expectedResult;
        };
        
        // Configure logger to capture execution time
        bool executionTimeLogged = false;
        
        _loggerMock.Setup(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(() => executionTimeLogged = true);
        
        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
        executionTimeLogged.Should().BeTrue();
    }
    
    // Simple test request class
    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; }
    }
}
