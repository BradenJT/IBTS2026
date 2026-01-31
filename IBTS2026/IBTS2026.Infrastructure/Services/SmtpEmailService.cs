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

    public async Task<bool> SendInvitationEmailAsync(
        string to,
        string inviterFirstName,
        string inviterLastName,
        string role,
        string invitationToken,
        DateTime expiresAt,
        CancellationToken ct)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:5001";
        var registrationUrl = $"{baseUrl}/register?token={invitationToken}";

        var emailBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>IBTS2026 Invitation</title>
            </head>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 20px;">
                    <h1 style="color: #2c3e50;">You've Been Invited!</h1>

                    <p>Hello,</p>

                    <p><strong>{inviterFirstName} {inviterLastName}</strong> has invited you to join IBTS2026 as a <strong>{role}</strong>.</p>

                    <p>Click the button below to complete your registration:</p>

                    <div style="text-align: center; margin: 30px 0;">
                        <a href="{registrationUrl}"
                           style="background-color: #3498db; color: white; padding: 12px 24px;
                                  text-decoration: none; border-radius: 5px; display: inline-block;">
                            Accept Invitation
                        </a>
                    </div>

                    <p style="color: #7f8c8d; font-size: 14px;">
                        This invitation will expire on <strong>{expiresAt:MMMM dd, yyyy at h:mm tt} UTC</strong>.
                    </p>

                    <p style="color: #7f8c8d; font-size: 14px;">
                        If you didn't expect this invitation or believe it was sent in error,
                        you can safely ignore this email.
                    </p>

                    <hr style="border: none; border-top: 1px solid #eee; margin: 30px 0;">

                    <p style="color: #95a5a6; font-size: 12px;">
                        If the button doesn't work, copy and paste this link into your browser:<br>
                        <a href="{registrationUrl}" style="color: #3498db;">{registrationUrl}</a>
                    </p>
                </div>
            </body>
            </html>
            """;

        return await SendEmailAsync(to, "You've been invited to join IBTS2026", emailBody, ct);
    }
}
