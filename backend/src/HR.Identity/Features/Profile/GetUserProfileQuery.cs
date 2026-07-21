namespace HR.Identity.Features.Profile;

using HR.Identity.Application.Dtos.UserProfile;

public record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileDto>;
