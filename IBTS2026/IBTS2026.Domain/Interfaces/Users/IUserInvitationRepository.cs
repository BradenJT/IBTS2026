using IBTS2026.Domain.Entities.Features.Users;

namespace IBTS2026.Domain.Interfaces.Users;

public interface IUserInvitationRepository
{
    Task<UserInvitation?> GetByIdAsync(int id, CancellationToken ct);
    Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken ct);
    Task<UserInvitation?> GetByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<UserInvitation>> GetPendingInvitationsAsync(CancellationToken ct);
    Task<IReadOnlyList<UserInvitation>> GetAllInvitationsAsync(CancellationToken ct);
    void Add(UserInvitation invitation);
    void Update(UserInvitation invitation);
    void Remove(UserInvitation invitation);
}
