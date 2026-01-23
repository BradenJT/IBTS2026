using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Users
{
    internal sealed class UserRepository(IBTS2026Context context)
                : RepositoryBase<User>(context.Users), IUserRepository
    {
        public Task<User?> GetByIdAsync(int id, CancellationToken ct)
            => Query().FirstOrDefaultAsync(u => u.UserId == id, ct);

        public void Add(User user) => AddEntity(user);

        public void Update(User user) => UpdateEntity(user);

        public void Remove(User user) => RemoveEntity(user);
    }
}
