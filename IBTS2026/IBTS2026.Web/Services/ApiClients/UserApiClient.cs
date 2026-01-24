using System.Net;
using System.Net.Http.Json;
using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients
{
    internal sealed class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserApiClient> _logger;

        public UserApiClient(HttpClient httpClient, ILogger<UserApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserModel?> GetUserAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserModel>(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                throw;
            }
        }

        public async Task<PagedResultModel<UserModel>> GetUsersAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (!string.IsNullOrWhiteSpace(sortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                if (!string.IsNullOrWhiteSpace(sortDir))
                    queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

                var url = $"/users?{string.Join("&", queryParams)}";
                var result = await _httpClient.GetFromJsonAsync<PagedResultModel<UserModel>>(url, ct);

                return result ?? new PagedResultModel<UserModel>([], 0, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw;
            }
        }

        public async Task<int> CreateUserAsync(CreateUserModel model, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/users", model, ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<int>(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserModel model, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/users/{id}", model, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/users/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                throw;
            }
        }
    }
}
