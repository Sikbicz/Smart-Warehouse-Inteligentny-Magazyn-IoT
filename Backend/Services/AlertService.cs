using Microsoft.Extensions.Logging;
using NewSmartWarehouseProject.Backend.Models;
using NewSmartWarehouseProject.Backend.Data;

namespace NewSmartWarehouseProject.Backend.Services;

public class AlertService
{
    private readonly ILogger<AlertService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly NotificationService _notificationService;

    public AlertService(ILogger<AlertService> logger, ApplicationDbContext dbContext, NotificationService notificationService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task AnalyzeSensorData(SensorPayload sensorData)
    {
        _logger.LogInformation($"Analyzing sensor data for DeviceId: {sensorData.DeviceId}");

        // Example: Trigger alert if temperature is too high
        if (sensorData.Temperature > 28.0) // Threshold for high temperature
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Message = $"High temperature detected on device {sensorData.DeviceId}: {sensorData.Temperature}°C",
                TriggeredAt = DateTime.UtcNow,
                IsActive = true
            };

            _dbContext.Alerts.Add(alert);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning($"Alert triggered: {alert.Message}");

            // Send SMS notification (placeholder)
            _notificationService.SendSmsNotification("+1234567890", alert.Message);
        }
        else
        {
            _logger.LogInformation("Temperature is within normal limits.");
        }
    }
}