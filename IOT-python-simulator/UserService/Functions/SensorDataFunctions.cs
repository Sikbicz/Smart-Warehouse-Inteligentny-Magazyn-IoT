using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Text;

using USERSERVICE.Models;
using USERSERVICE.Models.Sensor;
using USERSERVICE.Services.Alerting;

namespace USERSERVICE.Functions;

public class SensorDataFunctions
{
    private readonly ILogger<SensorDataFunctions> _logger;
    private readonly Container _container;
    private readonly IAlertService _alertService;

    public SensorDataFunctions(ILogger<SensorDataFunctions> logger, CosmosClient cosmosClient, IAlertService alertService)
    {
        _logger = logger;
        _container = cosmosClient.GetContainer("SmartWarehouseDB", "Odczyty");
        _alertService = alertService;
    }

    [Function("SensorDataService")]
    public async Task Run(
        [EventHubTrigger("iothub-ehub-iothub-sma-66048481-782fea9936", Connection = "IoTHubConnection", IsBatched = true)] EventData[] events)
    {
        foreach (var eventData in events)
        {
            try
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.ToArray());
                var payload = JsonConvert.DeserializeObject<SensorPayload>(messageBody);

                if (payload == null)
                {
                    _logger.LogWarning($"Nie udało się zdeserializować danych: {messageBody}");
                    continue;
                }

                if (!payload.Timestamp.HasValue && eventData.SystemProperties.TryGetValue("iothub-enqueuedtime", out var enqueuedTime))
                {
                    payload.Timestamp = (DateTime)enqueuedTime;
                }
                
                if (string.IsNullOrEmpty(payload.DeviceId) && eventData.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId))
                {
                    payload.DeviceId = deviceId.ToString();
                }
                
                if (string.IsNullOrEmpty(payload.DeviceId) || !payload.Timestamp.HasValue)
                {
                    _logger.LogError($"Brak DeviceId lub Timestamp w ładunku sensora. Ładunek: {messageBody}");
                    continue;
                }

                payload.Id = $"{payload.DeviceId}_{payload.Timestamp.Value:yyyyMMddHHmmssfff}";

                await _container.UpsertItemAsync(payload, new PartitionKey(payload.DeviceId));
                _logger.LogInformation($"Zapisano dane dla urządzenia {payload.DeviceId} do Cosmos DB.");

                if (payload.Temperature.HasValue)
                {
                    var tempReading = new SensorReading
                    {
                        Id = Guid.NewGuid(),
                        SensorId = payload.DeviceId,
                        Type = SensorType.Temperature,
                        Value = (decimal)payload.Temperature.Value,
                        Timestamp = payload.Timestamp.Value
                    };
                    await _alertService.CheckSensorReading(tempReading);
                }

                if (payload.Humidity.HasValue)
                {
                    var humidityReading = new SensorReading
                    {
                        Id = Guid.NewGuid(),
                        SensorId = payload.DeviceId,
                        Type = SensorType.Humidity,
                        Value = (decimal)payload.Humidity.Value,
                        Timestamp = payload.Timestamp.Value
                    };
                    await _alertService.CheckSensorReading(humidityReading);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Błąd podczas zapisu lub przetwarzania danych sensora. Dane wejściowe: {Encoding.UTF8.GetString(eventData.Body.ToArray())}");
            }
        }
    }
}
