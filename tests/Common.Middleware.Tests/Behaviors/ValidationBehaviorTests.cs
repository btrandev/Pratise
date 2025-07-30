using Common.Middleware.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System.Threading;
using Xunit;

namespace Common.Middleware.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<string> { }

    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            // This validator always passes
        }
    }

    public class FailingRequest : IRequest<string> { }

    public class FailingRequestValidator : AbstractValidator<FailingRequest>
    {
        public FailingRequestValidator()
        {
            RuleFor(x => x).Custom((_, context) =>
            {
                context.AddFailure(new ValidationFailure("PropertyName", "Error message"));
            });
        }
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var request = new TestRequest();
        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(x => x()).ReturnsAsync("Result");

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Result");
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validators = new List<IValidator<FailingRequest>> { new FailingRequestValidator() };
        var behavior = new ValidationBehavior<FailingRequest, string>(validators);

        var request = new FailingRequest();
        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(x => x()).ReturnsAsync("Result");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await behavior.Handle(request, nextMock.Object, CancellationToken.None);
        });

        nextMock.Verify(x => x(), Times.Never);
    }
}
