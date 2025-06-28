using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using NewSmartWarehouseProject.Backend.Models;
using NewSmartWarehouseProject.Backend.Data;
using NewSmartWarehouseProject.Backend.Services;
using Microsoft.Azure.Cosmos;

namespace NewSmartWarehouseProject.Backend;

public class SensorDataFunctions
{
    private readonly ILogger<SensorDataFunctions> _logger;
    private readonly ApplicationDbContext _sqlDbContext;
    private readonly AlertService _alertService;
    private readonly CosmosDbService _cosmosDbService;

    public SensorDataFunctions(ILogger<SensorDataFunctions> logger, ApplicationDbContext sqlDbContext, AlertService alertService, CosmosDbService cosmosDbService)
    {
        _logger = logger;
        _sqlDbContext = sqlDbContext;
        _alertService = alertService;
        _cosmosDbService = cosmosDbService;
    }

    [Function("SensorDataController")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("SensorDataController: Received a request.");

        try
        {
            var sensorPayload = await JsonSerializer.DeserializeAsync<SensorPayload>(req.Body);

            if (sensorPayload == null)
            {
                _logger.LogWarning("SensorDataController: Received null or invalid sensor payload.");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation($"SensorDataController: Received data for DeviceId: {sensorPayload.DeviceId}, Temperature: {sensorPayload.Temperature}, Humidity: {sensorPayload.Humidity}");

            // Set Id before adding to SQL DbContext
            sensorPayload.Id = Guid.NewGuid().ToString(); // Cosmos DB and SQL require an 'id' field

            // Save to SQL database (for alerts and user data)
            _sqlDbContext.SensorPayloads.Add(sensorPayload);
            await _sqlDbContext.SaveChangesAsync();
            _logger.LogInformation("Sensor data saved to SQL database.");

            // Save to Cosmos DB (for raw sensor data)
            await _cosmosDbService.CreateItemAsync(sensorPayload, new PartitionKey(sensorPayload.DeviceId));
            _logger.LogInformation("Sensor data saved to Cosmos DB.");

            // Pass to AlertService for analysis
            await _alertService.AnalyzeSensorData(sensorPayload);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            await response.WriteStringAsync("Sensor data processed successfully.");
            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"SensorDataController: JSON deserialization error: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"SensorDataController: An error occurred: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
    }
}
