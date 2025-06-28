using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NewSmartWarehouseProject.Backend.Data;
using NewSmartWarehouseProject.Backend.Models;

namespace NewSmartWarehouseProject.Backend;

public class DashboardFunctions
{
    private readonly ILogger<DashboardFunctions> _logger;
    private readonly ApplicationDbContext _dbContext;

    public DashboardFunctions(ILogger<DashboardFunctions> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Function("GetSensorData")]
    public async Task<HttpResponseData> GetSensorData(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dashboard/sensordata")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardFunctions: Received request for sensor data.");
        try
        {
            var sensorData = await _dbContext.SensorPayloads.ToListAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(sensorData);
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting sensor data: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
    }

    [Function("GetAlerts")]
    public async Task<HttpResponseData> GetAlerts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dashboard/alerts")] HttpRequestData req)
    {
        _logger.LogInformation("DashboardFunctions: Received request for alerts.");
        try
        {
            var alerts = await _dbContext.Alerts.ToListAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(alerts);
            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting alerts: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*"); // Add CORS header
            return errorResponse;
        }
    }

    [Function("DismissAlert")]
    public async Task<HttpResponseData> DismissAlert(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "dashboard/alerts/{alertId}/dismiss")] HttpRequestData req,
        string alertId)
    {
        _logger.LogInformation($"DashboardFunctions: Received request to dismiss alert: {alertId}");
        try
        {
            if (!Guid.TryParse(alertId, out Guid parsedAlertId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid alert ID format.");
                badRequestResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return badRequestResponse;
            }

            var alert = await _dbContext.Alerts.FirstOrDefaultAsync(a => a.Id == parsedAlertId);

            if (alert == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Alert with ID {alertId} not found.");
                notFoundResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return notFoundResponse;
            }

            alert.IsActive = false; // Set alert to inactive
            await _dbContext.SaveChangesAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Alert {alertId} dismissed successfully.");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error dismissing alert {alertId}: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            return errorResponse;
        }
    }
}