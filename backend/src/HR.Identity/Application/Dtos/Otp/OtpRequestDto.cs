namespace HR.Identity.Application.Dtos.Otp;

/// <summary>
/// Request DTO for OTP request
/// </summary>
public record OtpRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public int OtpType { get; init; }

    public OtpRequestDto(string email, string? phoneNumber = null, int otpType = 0)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        OtpType = otpType;
    }
}
