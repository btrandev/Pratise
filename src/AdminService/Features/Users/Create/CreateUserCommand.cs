using AdminService.Common.Results;
using AdminService.Features.Users.GetById;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.Create;

public record CreateUserCommand(CreateUserRequest Payload) : IRequest<Result<UserResponse>>;
