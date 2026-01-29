using System.Net.Http.Headers;
using IBTS2026.Web.Services.Auth;

namespace IBTS2026.Web.Services.Http;

public sealed class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthTokenStore _tokenStore;

    public AuthorizationMessageHandler(IAuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
