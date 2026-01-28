using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.Auth;

public interface IAuthService
{
    Task<UserModel?> GetCurrentUserAsync();
    Task LoginAsync(int userId);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    int? CurrentUserId { get; }
}
