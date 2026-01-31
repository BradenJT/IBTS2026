using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients;

public interface ILookupApiClient
{
    Task<List<PriorityModel>> GetPrioritiesAsync(CancellationToken ct = default);
    Task<List<StatusModel>> GetStatusesAsync(CancellationToken ct = default);
    Task<List<UserLookupModel>> GetUsersForDropdownAsync(CancellationToken ct = default);
}
