using Newtonsoft.Json;

// Umieszczamy klasy w przestrzeni nazw, aby uniknąć konfliktów.
namespace SmartWarehouse.Functions;

public class SensorPayload
{
    // Atrybut [JsonProperty("...")] mówi bibliotece, jak ma mapować
    // pola z dokumentu JSON w Cosmos DB na właściwości tej klasy.
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