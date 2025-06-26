using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using USERSERVICE.Services.Notifications;
using USERSERVICE.Data;
using USERSERVICE.Functions;
using USERSERVICE.Models;
using USERSERVICE.Services;
using USERSERVICE.Services.Alerting;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString")!;
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnectionString, sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            }));

        services.AddSingleton(s =>
        {
            var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
            if (string.IsNullOrEmpty(cosmosConnectionString))
            {
                throw new InvalidOperationException("CosmosDbConnectionString is not configured. Please set this environment variable.");
            }
            return new CosmosClient(cosmosConnectionString);
        });

        services.AddScoped<IUserService, USERSERVICE.Services.UserService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<INotificationService, NotificationService>(); 

    })
    .Build();

host.Run();
