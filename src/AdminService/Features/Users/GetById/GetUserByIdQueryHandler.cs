using AdminService.Domain.Repositories;
using AutoMapper;
using Common.Middleware.Results;
using MediatR;

namespace AdminService.Features.Users.GetById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            return Result<UserResponse>.Failure(new Error("User.NotFound", $"User with ID '{request.Id}' not found."));
        }

        var userResponse = new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Username,
            user.PhoneNumber,
            user.IsActive,
            user.EmailConfirmed,
            user.LastLoginAt,
            user.Role,
            user.TenantId,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedById,
            user.UpdatedById,
            user.CreatedByName,
            user.UpdatedByName
        );
        
        return Result<UserResponse>.Success(userResponse);
    }
}
