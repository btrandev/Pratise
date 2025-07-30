using Common.Middleware.Results;
using FluentAssertions;
using Xunit;

namespace AdminService.Tests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        var expectedData = "test data";
        
        // Act
        var result = Result<string>.Success(expectedData);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedData);
        result.Errors.Should().BeEmpty();
    }
    
    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = new Error("TestCode", "Error message");
        
        // Act
        var result = Result.Failure(error);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Code.Should().Be("TestCode");
        result.Errors.First().Message.Should().Be("Error message");
    }
    
    [Fact]
    public void Failure_WithStringError_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Error message";
        
        // Act
        var result = Result.Failure(errorMessage);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Message.Should().Be(errorMessage);
    }
    
    [Fact]
    public void AccessData_WhenResultIsFailure_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Failure("Error message");
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => { var value = result.Data; });
    }
}
