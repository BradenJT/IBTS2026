using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients
{
    public interface IUserApiClient
    {
        Task<UserModel?> GetUserAsync(int id, CancellationToken ct = default);
        Task<PagedResultModel<UserModel>> GetUsersAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default);
        Task<int> CreateUserAsync(CreateUserModel model, CancellationToken ct = default);
        Task<bool> UpdateUserAsync(int id, UpdateUserModel model, CancellationToken ct = default);
        Task<bool> DeleteUserAsync(int id, CancellationToken ct = default);
    }
}
