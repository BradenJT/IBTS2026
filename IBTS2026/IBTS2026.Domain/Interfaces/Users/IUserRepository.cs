using IBTS2026.Domain.Entities.Features.Users;

namespace IBTS2026.Domain.Interfaces.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken ct);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<bool> AnyUsersExistAsync(CancellationToken ct);
        void Add(User user);
        void Update(User user);
        void Remove(User user);
    }
}
