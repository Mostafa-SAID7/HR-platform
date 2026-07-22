namespace HR.Identity.Features.RequestOtp;

using MediatR;
using HR.Identity.Application.Dtos;

/// <summary>
/// Command to request an OTP
/// </summary>
public class RequestOtpCommand : IRequest<OtpResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int OtpType { get; set; }

    public RequestOtpCommand(string email, string? phoneNumber, int otpType)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        OtpType = otpType;
    }
}
