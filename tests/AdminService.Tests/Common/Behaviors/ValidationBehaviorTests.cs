using Common.Middleware.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdminService.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validatorMock;
    private readonly Mock<ILogger<ValidationBehavior<TestRequest, string>>> _loggerMock;
    private readonly ValidationBehavior<TestRequest, string> _behavior;
    
    public ValidationBehaviorTests()
    {
        _validatorMock = new Mock<IValidator<TestRequest>>();
        _loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, string>>>();
        
        _behavior = new ValidationBehavior<TestRequest, string>(
            new[] { _validatorMock.Object }
        );
    }
    
    [Fact]
    public async Task Handle_WhenValidationSucceeds_ShouldContinue()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var expectedResult = "Success";
        RequestHandlerDelegate<string> next = () => Task.FromResult(expectedResult);
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        result.Should().Be(expectedResult);
    }
    
    [Fact]
    public async Task Handle_WhenValidationFails_ShouldThrowValidationException()
    {
        // Arrange
        var request = new TestRequest { Name = string.Empty };
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };
        
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");
        
        _validatorMock.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _behavior.Handle(request, next, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldExecuteAllValidators()
    {
        // Arrange
        var request = new TestRequest { Name = "Test" };
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        
        validator1.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        validator2.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { validator1.Object, validator2.Object }
        );
        
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");
        
        // Act
        await behavior.Handle(request, next, CancellationToken.None);
        
        // Assert
        validator1.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        validator2.Verify(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenMultipleValidatorsHaveErrors_ShouldCombineAllErrors()
    {
        // Arrange
        var request = new TestRequest { Name = string.Empty };
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();
        
        var failures1 = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };
        
        var failures2 = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name must be at least 3 characters")
        };
        
        validator1.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures1));
        
        validator2.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures2));
        
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { validator1.Object, validator2.Object }
        );
        
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            behavior.Handle(request, next, CancellationToken.None));
        
        // The validation exception should contain both errors
        exception.Errors.Select(e => e.ErrorMessage)
            .Should().Contain(new[] { "Name is required", "Name must be at least 3 characters" });
    }
    
    // Simple test request class
    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; }
    }
}
