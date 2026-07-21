namespace HR.Identity.Features.Profile;

using MediatR;
using Microsoft.EntityFrameworkCore;
using HR.Identity.Application.Dtos.UserProfile;
using HR.Identity.Application.Mappings;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
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

        // Use centralized mapping instead of inline
        return user.ToProfileDto(roles);
    }
}
