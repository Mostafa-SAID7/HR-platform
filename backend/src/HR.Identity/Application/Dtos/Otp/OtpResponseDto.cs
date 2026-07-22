namespace HR.Identity.Application.Dtos.Otp;

/// <summary>
/// Response DTO for OTP request
/// </summary>
public record OtpResponseDto(
    Guid OtpRequestId,
    string Email,
    string Message,
    DateTime ExpiryTime);
