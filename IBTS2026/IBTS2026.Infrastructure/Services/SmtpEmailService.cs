using System.Net;
using System.Net.Mail;
using IBTS2026.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Infrastructure.Services;

internal sealed class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 25;
        var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@ibts2026.local";
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message, ct);

            _logger.LogInformation("Email sent successfully to {Recipient} with subject: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject: {Subject}", to, subject);
            return false;
        }
    }
}
