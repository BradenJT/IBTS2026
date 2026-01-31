using IBTS2026.Web.Models;
using IBTS2026.Web.Models.Auth;

namespace IBTS2026.Web.Services.Auth;

public interface IAuthService
{
    Task<UserModel?> GetCurrentUserAsync();
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password);
    Task<(bool Success, string? ErrorMessage)> RegisterAsync(string email, string password, string firstName, string lastName, string? invitationToken = null);
    Task<bool> IsFirstUserAsync();
    Task<InvitationInfoModel?> ValidateInvitationTokenAsync(string token);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    int? CurrentUserId { get; }
    string? CurrentUserRole { get; }
}

public record InvitationInfoModel(string Email, string Role, DateTime ExpiresAt);

