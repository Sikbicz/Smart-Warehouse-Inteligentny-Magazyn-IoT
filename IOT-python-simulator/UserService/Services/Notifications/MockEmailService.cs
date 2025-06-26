using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace USERSERVICE.Services.Notifications;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogWarning("--- SYMULACJA WYSYŁKI E-MAILA ---");
        _logger.LogWarning("Do: {To}", to);
        _logger.LogWarning("Temat: {Subject}", subject);
        _logger.LogWarning("Treść: {Body}", body);
        _logger.LogWarning("------------------------------------");

        await Task.Delay(50); 
    }
}
