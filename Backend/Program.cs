using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using NewSmartWarehouseProject.Backend.Services;
using NewSmartWarehouseProject.Backend.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Load from local.settings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        string sqlConnectionString = configuration.GetValue<string>("SqlConnectionString");
        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            throw new InvalidOperationException("SqlConnectionString is not configured.");
        }
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));

        services.AddSingleton<CosmosDbService>();

        services.AddScoped<AlertService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<UserService>();

    })
    .Build();

host.Run();