using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Added for IConfiguration

namespace NewSmartWarehouseProject.Backend.Services;

public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _cosmosContainer;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(ILogger<CosmosDbService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var cosmosConnectionString = configuration["CosmosDbConnectionString"]; // Read from IConfiguration
        if (string.IsNullOrEmpty(cosmosConnectionString))
        {
            throw new InvalidOperationException("CosmosDbConnectionString is not configured in app settings.");
        }

        var parts = cosmosConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Split('=', 2))
                                        .ToDictionary(p => p[0], p => p[1]);

        if (!parts.TryGetValue("AccountEndpoint", out var accountEndpoint) ||
            !parts.TryGetValue("AccountKey", out var accountKey))
        {
            throw new InvalidOperationException("CosmosDbConnectionString is missing AccountEndpoint or AccountKey.");
        }

        _cosmosClient = new CosmosClient(accountEndpoint, accountKey);
        _cosmosContainer = _cosmosClient.GetContainer("IoTData", "Telemetry"); // Assuming database "IoTData" and container "Telemetry"
    }

    public async Task CreateItemAsync<T>(T item, PartitionKey partitionKey)
    {
        try
        {
            await _cosmosContainer.CreateItemAsync(item, partitionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item in Cosmos DB.");
            throw; // Re-throw to propagate the error
        }
    }
}