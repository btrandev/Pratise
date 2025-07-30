using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.GetById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;
