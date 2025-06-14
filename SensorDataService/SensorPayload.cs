using Newtonsoft.Json;

namespace SmartWarehouse.Functions;

public class SensorPayload
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("deviceId")]
    public string? DeviceId { get; set; }

    [JsonProperty("temperature")]
    public double? Temperature { get; set; }

    [JsonProperty("humidity")]
    public double? Humidity { get; set; }

    [JsonProperty("timestamp")]
    public DateTime? Timestamp { get; set; }
}