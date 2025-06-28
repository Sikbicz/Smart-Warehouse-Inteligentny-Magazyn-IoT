using Newtonsoft.Json;

namespace NewSmartWarehouseProject.Backend.Models;

public class SensorPayload
{
    [JsonProperty("id")]
    public string? Id { get; set; } // Added for Cosmos DB
    [JsonProperty("deviceId")] // Added for Cosmos DB partition key
    public string? DeviceId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Alert
{
    public Guid Id { get; set; }
    public string? Message { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsActive { get; set; }
}