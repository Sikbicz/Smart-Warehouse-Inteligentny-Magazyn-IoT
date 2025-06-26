namespace USERSERVICE.Models.Sensor;

public class SensorReading
{
    public Guid Id { get; set; }
    public required string SensorId { get; set; }
    public SensorType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime Timestamp { get; set; }
}
