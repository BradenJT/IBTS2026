using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients
{
    public interface IIncidentApiClient
    {
        Task<IncidentDetailsModel?> GetIncidentAsync(int id, CancellationToken ct = default);
        Task<PagedResultModel<IncidentModel>> GetIncidentsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            int? statusId = null,
            int? priorityId = null,
            int? assignedToUserId = null,
            DateTime? createdAfter = null,
            DateTime? createdBefore = null,
            CancellationToken ct = default);
        Task<int> CreateIncidentAsync(CreateIncidentModel model, CancellationToken ct = default);
        Task<bool> UpdateIncidentAsync(int id, UpdateIncidentModel model, CancellationToken ct = default);
        Task<bool> DeleteIncidentAsync(int id, CancellationToken ct = default);
    }
}
