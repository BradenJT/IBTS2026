using IBTS2026.Web.Models.Auth;

namespace IBTS2026.Web.Services.ApiClients;

public interface IAuthApiClient
{
    Task<LoginResultModel?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<RegisterResultModel?> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
}
