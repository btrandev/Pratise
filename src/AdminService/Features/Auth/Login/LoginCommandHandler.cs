using AdminService.Domain.Entities;
using AdminService.Domain.Repositories;
using AdminService.Infrastructure.Authentication;
using Common.Middleware.Results;
using MediatR;
using Serilog;

namespace AdminService.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var payload = command.Payload;
        User? user;
        
        // Find user by email (and tenant ID if provided)
        if (payload.TenantId.HasValue && payload.TenantId != Guid.Empty)
        {
            user = await _userRepository.GetByEmailAndTenantAsync(payload.Email, payload.TenantId.Value);
        }
        else
        {
            user = await _userRepository.GetByEmailAsync(payload.Email);
        }

        // Check if user exists
        if (user == null)
        {
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Invalid email or password."));
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure(new Error("Auth.AccountDisabled", "User account is disabled."));
        }

        // Verify password
        if (!VerifyPassword(payload.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(new Error("Auth.InvalidCredentials", "Invalid email or password."));
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens and response
        var response = _jwtService.GenerateLoginResponse(user);
        return Result<LoginResponse>.Success(response);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
