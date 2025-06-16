using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Text; 

namespace SmartWarehouse.Functions;

public class SensorDataService
{
    private readonly ILogger _logger;
    private readonly Container _container;

    public SensorDataService(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        _logger = loggerFactory.CreateLogger<SensorDataService>();
        _container = cosmosClient.GetContainer("SmartWarehouseDB", "Odczyty");
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

                if (eventData.SystemProperties.TryGetValue("iothub-enqueuedtime", out var enqueuedTime))
                {
                    payload.Timestamp = (DateTime)enqueuedTime;
                }
                
                if (eventData.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId))
                {
                    payload.DeviceId = deviceId.ToString();
                }
                
                payload.Id = $"{payload.DeviceId}_{payload.Timestamp:yyyyMMddHHmmssfff}";

                await _container.UpsertItemAsync(payload, new PartitionKey(payload.DeviceId));

                _logger.LogInformation($"Zapisano dane dla urządzenia {payload.DeviceId} do Cosmos DB.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas zapisu: {ex.Message} | Dane wejściowe: {eventData.EventBody.ToString()}");
            }
        }
    }
}
