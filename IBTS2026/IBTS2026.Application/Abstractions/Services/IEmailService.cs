namespace IBTS2026.Application.Abstractions.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct);
    Task<bool> SendInvitationEmailAsync(
        string to,
        string inviterFirstName,
        string inviterLastName,
        string role,
        string invitationToken,
        DateTime expiresAt,
        CancellationToken ct);
}
