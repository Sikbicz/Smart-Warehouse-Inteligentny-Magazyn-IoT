using USERSERVICE.Models.Sensor;

namespace USERSERVICE.Services.Notifications;

public interface INotificationService
{
    Task SendAlertNotification(Alert alert);
}