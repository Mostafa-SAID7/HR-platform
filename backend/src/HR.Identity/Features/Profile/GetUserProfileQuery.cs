namespace HR.Identity.Features.Profile;

using HR.Identity.Application.Dtos;

/// <summary>
/// Query to get a user's profile.
/// </summary>
public record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileDto>;

/// <summary>
/// Handler for GetUserProfileQuery.
/// </summary>
public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userRepository = _unitOfWork.GetRepository<User>();
        var user = await userRepository.GetAsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            throw new NotFoundException("User", request.UserId);
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role?.Name ?? string.Empty)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList();

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Department = user.Department,
            JobTitle = user.JobTitle,
            Roles = roles,
            LastLoginUtc = user.LastLoginUtc ?? DateTime.UtcNow
        };
    }
}
