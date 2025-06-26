namespace USERSERVICE.Services.Notifications;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}