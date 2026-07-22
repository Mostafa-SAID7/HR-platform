namespace HR.Identity.Features.RequestOtp;

using MediatR;
using HR.Identity.Application.Dtos.Otp;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;
using HR.Identity.Domain;
using HR.Common;

/// <summary>
/// Handler for requesting OTP
/// </summary>
public class RequestOtpHandler : IRequestHandler<RequestOtpCommand, OtpResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpSender _otpSender;
    private readonly OtpOptions _otpOptions;
    private readonly ILogger<RequestOtpHandler> _logger;

    public RequestOtpHandler(
        IUnitOfWork unitOfWork,
        IOtpSender otpSender,
        IOptions<OtpOptions> otpOptions,
        ILogger<RequestOtpHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _otpSender = otpSender;
        _otpOptions = otpOptions.Value;
        _logger = logger;
    }

    public async Task<OtpResponseDto> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing OTP request for email: {Email}", request.Email);

            if (!_otpOptions.Enabled)
            {
                throw new InvalidOperationException("OTP service is not enabled");
            }

            // Find user by email or create if doesn't exist
            var userId = Guid.NewGuid(); // Placeholder - should lookup actual user
            var otpType = (OtpType)request.OtpType;

            var otpRequest = OtpRequest.Create(
                userId,
                request.Email,
                request.PhoneNumber,
                otpType,
                Guid.NewGuid()); // Use actual tenant ID

            // Send OTP via email (SendGrid only)
            _logger.LogInformation("Sending OTP email to: {Email}", request.Email);
            var sendSuccess = await _otpSender.SendOtpEmailAsync(
                request.Email,
                otpRequest.OtpCode,
                _otpOptions.ExpiryMinutes,
                cancellationToken);

            if (!sendSuccess)
            {
                _logger.LogError("Failed to send OTP for email: {Email}", request.Email);
                throw new InvalidOperationException("Failed to send OTP. Please try again.");
            }

            // Save OTP request to database
            _unitOfWork.GetRepository<OtpRequest>().Add(otpRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OTP sent successfully to {Email}", request.Email);

            return new OtpResponseDto(
                otpRequest.Id,
                request.Email,
                "OTP sent successfully",
                otpRequest.ExpiryTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting OTP for email: {Email}", request.Email);
            throw;
        }
    }
}
