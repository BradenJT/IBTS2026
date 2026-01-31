using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Users;

internal sealed class UserInvitationRepository(IBTS2026Context context)
    : RepositoryBase<UserInvitation>(context.UserInvitations), IUserInvitationRepository
{
    public Task<UserInvitation?> GetByIdAsync(int id, CancellationToken ct)
        => Query().FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<UserInvitation?> GetByTokenAsync(string token, CancellationToken ct)
        => Query().FirstOrDefaultAsync(i => i.Token == token, ct);

    public Task<UserInvitation?> GetByEmailAsync(string email, CancellationToken ct)
        => Query()
            .Where(i => i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<UserInvitation>> GetPendingInvitationsAsync(CancellationToken ct)
        => await Query()
            .Where(i => !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<UserInvitation>> GetAllInvitationsAsync(CancellationToken ct)
        => await Query()
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

    public void Add(UserInvitation invitation) => AddEntity(invitation);

    public void Update(UserInvitation invitation) => UpdateEntity(invitation);

    public void Remove(UserInvitation invitation) => RemoveEntity(invitation);
}
