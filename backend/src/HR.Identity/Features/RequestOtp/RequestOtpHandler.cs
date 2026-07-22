namespace HR.Identity.Features.RequestOtp;

using MediatR;
using HR.Identity.Application.Dtos;
using HR.Identity.Domain;
using HR.Common;

/// <summary>
/// Handler for requesting OTP
/// </summary>
public class RequestOtpHandler : IRequestHandler<RequestOtpCommand, OtpResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestOtpHandler> _logger;

    public RequestOtpHandler(IUnitOfWork unitOfWork, ILogger<RequestOtpHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OtpResponseDto> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing OTP request for email: {Email}", request.Email);

            // Find user by email or create if doesn't exist
            // Note: This is a simplified example - implement proper user lookup
            var userId = Guid.NewGuid(); // Placeholder
            var otpType = (OtpType)request.OtpType;

            var otpRequest = OtpRequest.Create(
                userId,
                request.Email,
                request.PhoneNumber,
                otpType,
                Guid.NewGuid()); // Use actual tenant ID

            // TODO: Send OTP via email/SMS
            _logger.LogInformation("OTP generated: {OtpCode} for email: {Email}", otpRequest.OtpCode, request.Email);

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
