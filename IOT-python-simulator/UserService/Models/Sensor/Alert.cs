namespace USERSERVICE.Models.Sensor;

public class Alert
{
    public Guid Id { get; set; }
    public required string SensorId { get; set; }
    public SensorType SensorType { get; set; }
    public decimal ReadingValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public bool IsActive { get; set; }
    public string? AcknowledgedByUserId { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}
