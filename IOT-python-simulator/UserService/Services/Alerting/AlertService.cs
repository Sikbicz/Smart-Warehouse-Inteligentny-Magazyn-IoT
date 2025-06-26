using Microsoft.Extensions.Logging;
using USERSERVICE.Data;
using USERSERVICE.Models.Sensor;
using USERSERVICE.Services.Notifications;

namespace USERSERVICE.Services.Alerting;

public class AlertService : IAlertService
{
    private readonly ILogger<AlertService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;

    private const decimal MaxTemperature = 25.0M; // Stopnie Celsjusza
    private const decimal MaxHumidity = 60.0M;    // Procent

    public AlertService(
        ILogger<AlertService> logger,
        ApplicationDbContext dbContext,
        INotificationService notificationService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task CheckSensorReading(SensorReading reading)
    {
        _logger.LogInformation("Sprawdzanie odczytu sensora {SensorId} ({SensorType}): {Value}",
            reading.SensorId, reading.Type, reading.Value);

        bool thresholdExceeded = false;
        string alertMessage = string.Empty;
        decimal thresholdValue = 0;

        switch (reading.Type)
        {
            case SensorType.Temperature:
                if (reading.Value > MaxTemperature)
                {
                    thresholdExceeded = true;
                    alertMessage = $"TEMPERATURA KRYTYCZNIE WYSOKA! {reading.Value}°C (max: {MaxTemperature}°C) w magazynie {reading.SensorId}.";
                    thresholdValue = MaxTemperature;
                }
                break;
            case SensorType.Humidity:
                if (reading.Value > MaxHumidity)
                {
                    thresholdExceeded = true;
                    alertMessage = $"WILGOTNOŚĆ KRYTYCZNIE WYSOKA! {reading.Value}% (max: {MaxHumidity}%) w magazynie {reading.SensorId}.";
                    thresholdValue = MaxHumidity;
                }
                break;
            default:
                _logger.LogWarning("Nieznany typ sensora: {SensorType}. Odczyt z ignorowany.", reading.Type);
                break;
        }

        if (thresholdExceeded)
        {
            _logger.LogWarning("Przekroczenie progu wykryte: {Message}", alertMessage);

            var newAlert = new Alert
            {
                Id = Guid.NewGuid(),
                SensorId = reading.SensorId,
                SensorType = reading.Type,
                ReadingValue = reading.Value,
                ThresholdValue = thresholdValue,
                Message = alertMessage,
                TriggeredAt = DateTime.UtcNow,
                IsActive = true
            };

            await _dbContext.Alerts.AddAsync(newAlert);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Alert zapisany w bazie danych. Id: {AlertId}", newAlert.Id);

            await _notificationService.SendAlertNotification(newAlert);
            _logger.LogInformation("Powiadomienie o alercie zainicjowane (mockowe).");
        }
    }
}
