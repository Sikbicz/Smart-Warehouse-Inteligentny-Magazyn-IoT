using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using USERSERVICE.Data;
using USERSERVICE.Models;
using USERSERVICE.Models.Sensor;
using Microsoft.EntityFrameworkCore;

namespace USERSERVICE.Functions;

public class DashboardFunctions
{
    private readonly ILogger<DashboardFunctions> _logger;
    private readonly Container _cosmosContainer;
    private readonly ApplicationDbContext _sqlDbContext;

    public DashboardFunctions(ILogger<DashboardFunctions> logger, CosmosClient cosmosClient, ApplicationDbContext sqlDbContext)
    {
        _logger = logger;
        _cosmosContainer = cosmosClient.GetContainer("SmartWarehouseDB", "Odczyty");
        _sqlDbContext = sqlDbContext;
    }

    [Function("GetSensorDataByDateRange")]
    public async Task<HttpResponseData> GetSensorDataByDateRange(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/sensordata")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardFunctions: Otrzymano zapytanie o dane sensorów z zakresu dat.");
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

            var iterator = _cosmosContainer.GetItemQueryIterator<SensorPayload>(queryDefinition);
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
        _logger.LogInformation("DashboardFunctions: Otrzymano zapytanie o najnowsze dane sensorów.");
        try
        {
            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c ORDER BY c.timestamp DESC");

            var iterator = _cosmosContainer.GetItemQueryIterator<SensorPayload>(queryDefinition);
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

    [Function("GetActiveAlerts")]
    public async Task<HttpResponseData> GetActiveAlerts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/alerts/active")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardFunctions: Otrzymano zapytanie o aktywne alerty.");
        try
        {
            var activeAlerts = await _sqlDbContext.Alerts
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(activeAlerts);
            return okResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Błąd w GetActiveAlerts: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera podczas pobierania alertów.");
            return errorResponse;
        }
    }

    [Function("GetAllAlerts")]
    public async Task<HttpResponseData> GetAllAlerts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/alerts/all")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardFunctions: Otrzymano zapytanie o wszystkie alerty.");
        try
        {
            var allAlerts = await _sqlDbContext.Alerts
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync();

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(allAlerts);
            return okResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Błąd w GetAllAlerts: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera podczas pobierania wszystkich alertów.");
            return errorResponse;
        }
    }
    [Function("AcknowledgeAlert")]
    public async Task<HttpResponseData> AcknowledgeAlert(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "dashboard/alerts/{alertId}/acknowledge")] HttpRequestData req,
        string alertId)
    {
        _logger.LogInformation($"DashboardFunctions: Otrzymano zapytanie o potwierdzenie alertu: {alertId}");

        // Sprawdź, czy przekazane ID jest prawidłowym GUID
        if (!Guid.TryParse(alertId, out Guid parsedGuid))
        {
            _logger.LogWarning($"Nieprawidłowy format GUID w zapytaniu: {alertId}");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Nieprawidłowy format ID alertu.");
            return badRequestResponse;
        }

        try
        {
            // Znajdź alert w bazie danych
            var alertToAcknowledge = await _sqlDbContext.Alerts.FindAsync(parsedGuid);

            if (alertToAcknowledge == null)
            {
                _logger.LogWarning($"Nie znaleziono alertu o ID: {alertId}");
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Alert o podanym ID nie został znaleziony.");
                return notFoundResponse;
            }

            // Zmień status alertu
            alertToAcknowledge.IsActive = false;
            alertToAcknowledge.AcknowledgedAt = DateTime.UtcNow;

            await _sqlDbContext.SaveChangesAsync(); // Zapisz zmiany w bazie danych

            _logger.LogInformation($"Alert {alertId} został pomyślnie potwierdzony.");
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Wystąpił błąd podczas potwierdzania alertu: {alertId}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera podczas potwierdzania alertu.");
            return errorResponse;
        }
    }
    [Function("GetHistoricalData")]
public async Task<HttpResponseData> GetHistoricalData(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard/historical")] HttpRequestData req)
{
    _logger.LogInformation("DashboardFunctions: Otrzymano zapytanie o dane historyczne do wykresu.");

    try
    {
        // Ustaw datę początkową na 7 dni temu
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("o"); // Format ISO 8601

        // Zapytanie do Cosmos DB o wszystkie odczyty z ostatnich 7 dni
        var queryDefinition = new QueryDefinition("SELECT c.timestamp, c.temperature, c.humidity FROM c WHERE c.timestamp >= @startDate")
            .WithParameter("@startDate", startDate);

        var iterator = _cosmosContainer.GetItemQueryIterator<SensorPayload>(queryDefinition);
        var readings = new List<SensorPayload>();

        // Pobierz wszystkie pasujące dokumenty
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            readings.AddRange(response.ToList());
        }
        var dailyAverages = readings
            .Where(r => r.Timestamp.HasValue && r.Temperature.HasValue)
            .GroupBy(r => r.Timestamp.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                AverageTemperature = g.Average(r => r.Temperature.Value)
            })
            .OrderBy(result => result.Date)
            .ToList();

        // Przygotuj ostateczny obiekt do wysłania
        var chartData = new
        {
            labels = dailyAverages.Select(d => d.Date.ToString("yyyy-MM-dd")).ToList(),
            temperatures = dailyAverages.Select(d => Math.Round(d.AverageTemperature, 2)).ToList()
        };

        var okResponse = req.CreateResponse(HttpStatusCode.OK);
        await okResponse.WriteAsJsonAsync(chartData);
        return okResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Błąd w GetHistoricalData.");
        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
        await errorResponse.WriteStringAsync("Wystąpił wewnętrzny błąd serwera podczas pobierania danych historycznych.");
        return errorResponse;
    }
}



    
}
