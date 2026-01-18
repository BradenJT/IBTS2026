using IBTS2026.Domain.Entities;

namespace IBTS2026.Domain.Interfaces.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct);
        void Add(User user);
        void Update(User user);
        void Remove(User user);
    }
}
