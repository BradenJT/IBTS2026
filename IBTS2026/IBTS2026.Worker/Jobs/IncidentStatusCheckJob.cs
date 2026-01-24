using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Models.Requests;

namespace IBTS2026.Worker.Jobs
{
    /// <summary>
    /// Example background job that periodically checks for stale incidents.
    /// Uses Application layer handlers directly (no API dependency).
    /// </summary>
    public sealed class IncidentStatusCheckJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IncidentStatusCheckJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public IncidentStatusCheckJob(
            IServiceScopeFactory scopeFactory,
            ILogger<IncidentStatusCheckJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Incident Status Check Job started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckStaleIncidentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking stale incidents.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Incident Status Check Job stopped.");
        }

        private async Task CheckStaleIncidentsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IRequestDispatcher>();

            _logger.LogDebug("Checking for stale incidents...");

            // Query for incidents that have been open for more than 7 days
            var staleDate = DateTime.UtcNow.AddDays(-7);
            var query = new GetIncidentsQuery(
                new PageRequest(1, 100),
                null,
                null,
                StatusId: 1, // Open status
                PriorityId: null,
                AssignedToUserId: null,
                CreatedAfter: null,
                CreatedBefore: staleDate);

            var result = await dispatcher.SendAsync<GetIncidentsQuery, PagedResult<IncidentDto>>(query, ct);

            if (result.TotalCount > 0)
            {
                _logger.LogWarning(
                    "Found {Count} stale incidents (open for more than 7 days).",
                    result.TotalCount);

                foreach (var incident in result.Items)
                {
                    _logger.LogWarning(
                        "Stale incident: ID={IncidentId}, Title={Title}, Created={CreatedAt}",
                        incident.IncidentId,
                        incident.Title,
                        incident.CreatedAt);
                }
            }
            else
            {
                _logger.LogDebug("No stale incidents found.");
            }
        }
    }
}
