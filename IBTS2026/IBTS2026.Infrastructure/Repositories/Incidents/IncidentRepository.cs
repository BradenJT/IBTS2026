using IBTS2026.Domain.Entities.Features.Incidents.Incident;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Incidents
{
    internal sealed class IncidentRepository : RepositoryBase<Incident>, IIncidentRepository
    {
        public IncidentRepository(IBTS2026Context context)
            : base(context.Incidents)
        {
        }

        public Task<Incident?> GetByIdAsync(int id, CancellationToken ct)
            => Query().FirstOrDefaultAsync(i => i.IncidentId == id, ct);

        public Task<Incident?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
            => Query()
                .Include(i => i.Status)
                .Include(i => i.Priority)
                .Include(i => i.CreatedByUser)
                .FirstOrDefaultAsync(i => i.IncidentId == id, ct);

        public void Add(Incident incident) => AddEntity(incident);

        public void Update(Incident incident) => UpdateEntity(incident);

        public void Remove(Incident incident) => RemoveEntity(incident);
    }
}
