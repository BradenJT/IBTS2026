using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Users
{
    internal sealed class UserRepository 
        : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(IBTS2026Context context)
            : base(context.Users)
        {
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken ct)
            => Query().FirstOrDefaultAsync(u => u.UserId == id, ct);

        public void Add(User user) => AddEntity(user);

        public void Update(User user) => UpdateEntity(user);

        public void Remove(User user) => RemoveEntity(user);
    }
}
