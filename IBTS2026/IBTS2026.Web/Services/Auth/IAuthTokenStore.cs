namespace IBTS2026.Web.Services.Auth;

public interface IAuthTokenStore
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
}
