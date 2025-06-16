using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton((provider) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnection", EnvironmentVariableTarget.Process);
            if(string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Brak klucza 'CosmosDBConnection' w ustawieniach. Sprawdź plik local.settings.json.");
            }
            return new CosmosClient(connectionString);
        });
    })
    .Build();

host.Run();
