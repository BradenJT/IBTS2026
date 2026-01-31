namespace IBTS2026.Web.Services.ApiClients;

public interface IInvitationApiClient
{
    Task<IReadOnlyList<InvitationModel>> GetInvitationsAsync(CancellationToken ct = default);
    Task<InvitationModel?> SendInvitationAsync(string email, string role, CancellationToken ct = default);
    Task<bool> CancelInvitationAsync(int id, CancellationToken ct = default);
    Task<bool> ResendInvitationAsync(int id, CancellationToken ct = default);
}

public record InvitationModel(
    int Id,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsUsed,
    DateTime? UsedAt,
    bool IsValid,
    string InvitedByName);
