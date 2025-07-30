using AdminService.Common.Results;
using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Features.Auth.Login;
using AdminService.Infrastructure.Authentication;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Serilog;
using Xunit;

namespace AdminService.Tests.Features.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IValidator<LoginCommand>> _validatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _validatorMock = new Mock<IValidator<LoginCommand>>();
        _mapperMock = new Mock<IMapper>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object);

        // Setup default validation to pass
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnValidationError()
    {
        // Arrange
        var command = new LoginCommand
        (new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email is required")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be("Email is required");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnError()
    {
        // Arrange
        var command = new LoginCommand
        (new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Payload.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ShouldReturnError()
    {
        // Arrange
        var command = new LoginCommand
        (new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Payload.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("differentPassword"),
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Payload.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldReturnError()
    {
        // Arrange
        var command = new LoginCommand
        (new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Payload.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Payload.Password),
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Payload.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be("User account is disabled");
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ShouldGenerateTokenAndReturnSuccess()
    {
        // Arrange
        var command = new LoginCommand
        (new LoginRequest
        {
            Email = "test@example.com",
            Password = "password"
        });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Payload.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Payload.Password),
            IsActive = true,
            Claims = new List<UserClaim>
            {
                new UserClaim
                {
                    ClaimType = "permission",
                    ClaimValue = "Users.View"
                }
            }
        };

        var expectedResponse = new LoginResponse
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new LoginResponse.UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = "Test",
                LastName = "User",
                Role = "User",
                TenantId = user.TenantId
            }
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Payload.Email))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.GenerateLoginResponse(user))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);

        _jwtServiceMock.Verify(x => x.GenerateLoginResponse(user), Times.Once);
    }
}
