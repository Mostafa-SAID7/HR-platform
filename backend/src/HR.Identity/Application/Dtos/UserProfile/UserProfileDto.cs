namespace HR.Identity.Application.Dtos.UserProfile;

/// <summary>
/// User profile DTO.
/// </summary>
public record UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public List<string> Roles { get; set; } = [];
    public DateTime LastLoginUtc { get; set; }
}
