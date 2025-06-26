using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using USERSERVICE.Services.Notifications; // Upewnij się, że ten using jest poprawny

namespace USERSERVICE.Functions;

public class TestFunctions
{
    private readonly ILogger<TestFunctions> _logger;
    private readonly IEmailService _emailService; // Wstrzykujemy IEmailService

    public TestFunctions(ILogger<TestFunctions> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [Function("TestSendEmail")]
    public async Task<HttpResponseData> TestSendEmail(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "test/send-email")] HttpRequestData req)
    {
        _logger.LogInformation("Rozpoczynanie testu symulacji wysyłki e-maila.");

        try
        {
            var toEmail = "test-recipient@example.com";
            var subject = "Testowy e-mail z symulacji";
            var body = "Ta wiadomość nie została wysłana, ale jej treść pojawiła się w logach.";

            await _emailService.SendEmailAsync(toEmail, subject, body);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Symulacja wysłania e-maila zakończona pomyślnie. Sprawdź logi w konsoli.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd podczas symulacji wysyłki e-maila.");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Błąd podczas symulacji: {ex.Message}");
            return errorResponse;
        }
    }
}
