namespace IBTS2026.Application.Abstractions.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct);
}
