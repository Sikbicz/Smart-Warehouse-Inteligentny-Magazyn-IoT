using Microsoft.Extensions.Logging;

namespace NewSmartWarehouseProject.Backend.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public void SendSmsNotification(string phoneNumber, string message)
    {
        _logger.LogInformation($"[MOCK SMS] Sending SMS to {phoneNumber}: {message}");
        // In a real application, integrate with an SMS gateway here.
    }

    public void SendEmailNotification(string recipientEmail, string subject, string body)
    {
        _logger.LogInformation($"[MOCK EMAIL] Sending email to {recipientEmail} with subject '{subject}': {body}");
        // In a real application, integrate with an email service here.
    }
}