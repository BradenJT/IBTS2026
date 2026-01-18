using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Users;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Users
{
    internal sealed class UserRepository 
        : Repository<User>, IUserRepository
    {
        public UserRepository(IBTS2026Context context)
            : base(context.Users)
        {
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken ct)
        {
            return DbSet.FirstOrDefaultAsync(u => u.UserId == id, ct);
        }

        public void Add(User user)
        {
            DbSet.Add(user);
        }

        public void Update(User user)
        {
            DbSet.Update(user);
        }

        public void Remove(User user)
        {
            DbSet.Remove(user);
        }
    }
}
