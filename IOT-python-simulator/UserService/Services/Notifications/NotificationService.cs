using Microsoft.Extensions.Logging;
using USERSERVICE.Models.Sensor;

namespace USERSERVICE.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IEmailService _emailService;

    public NotificationService(
        ILogger<NotificationService> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task SendAlertNotification(Alert alert)
    {
        _logger.LogInformation("Przygotowywanie powiadomienia o alercie dla sensora {SensorId}", alert.SensorId);

        var adminEmail = "admin@smartwarehouse.com";
        var managerEmail = "manager@smartwarehouse.com";

        var subject = $"KRYTYCZNY ALERT MAGAZYN: {alert.SensorType} przekroczony!";
        var body = $"ALERT: {alert.Message}\nOdczyt: {alert.ReadingValue}\nPróg: {alert.ThresholdValue}\nCzas: {alert.TriggeredAt:yyyy-MM-dd HH:mm:ss}";

        try
        {
            await _emailService.SendEmailAsync(adminEmail, subject, body);
            await _emailService.SendEmailAsync(managerEmail, subject, body);
            _logger.LogInformation("E-maile z alertem (mockowe) zainicjowane.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas inicjowania mockowej wysyłki e-maili.");
        }
    }
}