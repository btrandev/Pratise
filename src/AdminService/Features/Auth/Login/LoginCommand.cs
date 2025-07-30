using AdminService.Common.Results;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Auth.Login;

public record LoginCommand(LoginRequest Payload) : IRequest<Result<LoginResponse>>;
