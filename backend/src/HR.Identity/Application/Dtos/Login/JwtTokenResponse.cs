namespace HR.Identity.Application.Dtos;

/// <summary>
/// JWT token response DTO - returned after successful login
/// </summary>
public record JwtTokenResponse(
    string Token,
    int ExpiresIn);
