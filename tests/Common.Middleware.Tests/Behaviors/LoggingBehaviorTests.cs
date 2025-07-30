using Common.Middleware.Behaviors;
using MediatR;
using Moq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using System.Threading;
using Xunit;

namespace Common.Middleware.Tests.Behaviors;

public class LoggingBehaviorTests
{
    public class TestRequest : IRequest<string> 
    {
        public string Name { get; set; } = "Test";
    }

    [Fact]
    public async Task Handle_ShouldLogRequestAndResponse()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TestCorrelator()
                .CreateLogger();

            var behavior = new LoggingBehavior<TestRequest, string>(logger);
            var request = new TestRequest();

            var nextMock = new Mock<RequestHandlerDelegate<string>>();
            nextMock.Setup(x => x()).ReturnsAsync("Result");

            // Act
            var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

            // Assert
            var logs = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            
            // Verify we have at least 2 logs (request start and end)
            Assert.True(logs.Count >= 2);
            
            // Check for request start log
            Assert.Contains(logs, log => 
                log.Level == LogEventLevel.Information && 
                log.MessageTemplate.Text.Contains("Starting request") &&
                log.Properties.ContainsKey("RequestType") &&
                log.Properties.ContainsKey("Request"));
            
            // Check for request completion log
            Assert.Contains(logs, log => 
                log.Level == LogEventLevel.Information && 
                log.MessageTemplate.Text.Contains("Completed request") &&
                log.Properties.ContainsKey("RequestType") &&
                log.Properties.ContainsKey("ElapsedMilliseconds"));
        }
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.TestCorrelator()
                .CreateLogger();

            var behavior = new LoggingBehavior<TestRequest, string>(logger);
            var request = new TestRequest();

            var exception = new Exception("Test exception");
            var nextMock = new Mock<RequestHandlerDelegate<string>>();
            nextMock.Setup(x => x()).ThrowsAsync(exception);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await behavior.Handle(request, nextMock.Object, CancellationToken.None);
            });

            var logs = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            
            // Check for error log
            Assert.Contains(logs, log => 
                log.Level == LogEventLevel.Error && 
                log.MessageTemplate.Text.Contains("Error handling request") &&
                log.Properties.ContainsKey("RequestType") &&
                log.Properties.ContainsKey("Exception"));
        }
    }
}
