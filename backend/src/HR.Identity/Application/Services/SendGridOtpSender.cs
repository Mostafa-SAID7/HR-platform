namespace HR.Identity.Application.Services;

using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using HR.Identity.Application.Interfaces;
using HR.Identity.Application.Options;

/// <summary>
/// SendGrid-based OTP sender implementation (email only)
/// </summary>
public class SendGridOtpSender : IOtpSender
{
    private readonly SendGridClient _client;
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridOtpSender> _logger;

    public SendGridOtpSender(IOptions<SendGridOptions> options, ILogger<SendGridOtpSender> logger)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(email);
            var subject = "Your OTP Code - HR Platform";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Your One-Time Password (OTP)</h2>
                        <p>Your OTP code is: <strong style='font-size: 24px; color: #007bff;'>{otpCode}</strong></p>
                        <p>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
                        <p>If you didn't request this code, please ignore this email.</p>
                        <hr />
                        <p style='color: #666; font-size: 12px;'>HR Platform - Identity Service</p>
                    </body>
                </html>";

            var plainTextContent = $"Your OTP code is: {otpCode}. This code will expire in {expiryMinutes} minutes.";

            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                PlainTextContent = plainTextContent,
                HtmlContent = htmlContent
            };

            msg.AddTo(to);

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("OTP email sent successfully to {Email}", email);
                return true;
            }

            _logger.LogError("Failed to send OTP email to {Email}. Status: {StatusCode}", email, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP email to {Email}", email);
            return false;
        }
    }
}

public class SendGridOtpSender : IOtpSender
{
    private readonly SendGridClient _client;
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridOtpSender> _logger;

    public SendGridOtpSender(IOptions<SendGridOptions> options, ILogger<SendGridOtpSender> logger)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(email);
            var subject = "Your OTP Code - HR Platform";
            var htmlContent = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Your One-Time Password (OTP)</h2>
                        <p>Your OTP code is: <strong style='font-size: 24px; color: #007bff;'>{otpCode}</strong></p>
                        <p>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
                        <p>If you didn't request this code, please ignore this email.</p>
                        <hr />
                        <p style='color: #666; font-size: 12px;'>HR Platform - Identity Service</p>
                    </body>
                </html>";

            var plainTextContent = $"Your OTP code is: {otpCode}. This code will expire in {expiryMinutes} minutes.";

            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                PlainTextContent = plainTextContent,
                HtmlContent = htmlContent
            };

            msg.AddTo(to);

            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("OTP email sent successfully to {Email}", email);
                return true;
            }

            _logger.LogError("Failed to send OTP email to {Email}. Status: {StatusCode}", email, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP email to {Email}", email);
            return false;
        }
    }
}
