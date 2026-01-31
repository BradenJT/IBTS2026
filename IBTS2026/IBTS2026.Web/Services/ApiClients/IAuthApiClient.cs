using IBTS2026.Web.Models.Auth;
using IBTS2026.Web.Services.Auth;

namespace IBTS2026.Web.Services.ApiClients;

public interface IAuthApiClient
{
    Task<LoginResultModel?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<RegisterResultModel?> RegisterAsync(string email, string password, string firstName, string lastName, string? invitationToken = null, CancellationToken ct = default);
    Task<bool> IsFirstUserAsync(CancellationToken ct = default);
    Task<InvitationInfoModel?> ValidateInvitationTokenAsync(string token, CancellationToken ct = default);
    Task<bool> ValidateSecurityStampAsync(int userId, string securityStamp, CancellationToken ct = default);
}
