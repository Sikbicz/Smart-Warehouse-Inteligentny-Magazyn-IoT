using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Web;

namespace SmartWarehouse.Functions;

public class DashboardController 
{
    private readonly ILogger _logger;
    private readonly Container _container;

    public DashboardController(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        _logger = loggerFactory.CreateLogger<DashboardController>();
        _container = cosmosClient.GetContainer("SmartWarehouseDB", "Odczyty");
    }

    [Function("GetSensorDataByDateRange")]
    public async Task<HttpResponseData> GetSensorDataByDateRange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/sensordata")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardController: Otrzymano zapytanie o dane sensorów z zakresu dat.");
        try
        {
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string? startDateStr = query["startDate"];
            string? endDateStr = query["endDate"];

            if (string.IsNullOrEmpty(startDateStr) || string.IsNullOrEmpty(endDateStr))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Proszę podać parametry 'startDate' i 'endDate' w formacie ISO 8601.");
                return badRequestResponse;
            }

            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.timestamp >= @startDate AND c.timestamp <= @endDate ORDER BY c.timestamp DESC")
                .WithParameter("@startDate", DateTime.Parse(startDateStr))
                .WithParameter("@endDate", DateTime.Parse(endDateStr));
            
            var iterator = _container.GetItemQueryIterator<SensorPayload>(queryDefinition);
            var results = new List<SensorPayload>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(results);
            return okResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Błąd w GetSensorDataByDateRange: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera.");
            return errorResponse;
        }
    }

    [Function("GetLatestSensorData")]
    public async Task<HttpResponseData> GetLatestSensorData(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/latest")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardController: Otrzymano zapytanie o najnowsze dane sensorów.");
        try
        {
            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c ORDER BY c.timestamp DESC");

            var iterator = _container.GetItemQueryIterator<SensorPayload>(queryDefinition);
            var results = new List<SensorPayload>();
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Any())
            {
                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                await okResponse.WriteAsJsonAsync(results.First()); 
                return okResponse;
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Błąd w GetLatestSensorData: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera.");
            return errorResponse;
        }
    }
}
