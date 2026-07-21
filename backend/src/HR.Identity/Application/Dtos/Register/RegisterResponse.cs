namespace HR.Identity.Application.Dtos.Register;

/// <summary>
/// Register response DTO.
/// </summary>
public record RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
