namespace HR.Notification.Application.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

/// <summary>
/// Email notification service using SendGrid
/// </summary>
public class EmailNotificationService : INotificationChannelService
{
    private readonly SendGrid.SendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        var sendGridKey = configuration["SendGrid:ApiKey"] ?? throw new InvalidOperationException("SendGrid API key is required");
        _sendGridClient = new SendGrid.SendGridClient(sendGridKey);
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@hrplatform.com";
        _logger = logger;
    }

    public async Task<bool> SendAsync(string recipient, string title, string content, CancellationToken cancellationToken)
    {
        try
        {
            var from = new SendGrid.Helpers.Mail.EmailAddress(_fromEmail, "HR Platform");
            var to = new SendGrid.Helpers.Mail.EmailAddress(recipient);
            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, title, content, content);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogInformation("Email sent successfully to {Email}", recipient);
                return true;
            }

            _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}", recipient, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", recipient);
            return false;
        }
    }
}
