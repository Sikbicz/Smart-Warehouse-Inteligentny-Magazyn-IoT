using USERSERVICE.Models.Sensor;

namespace USERSERVICE.Services.Alerting;

public interface IAlertService
{
    Task CheckSensorReading(SensorReading reading);
}
