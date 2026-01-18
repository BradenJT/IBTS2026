using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Incidents;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Incidents
{
    internal sealed class IncidentRepository : Repository<Incident>, IIncidentRepository
    {
        public IncidentRepository(IBTS2026Context context) 
            : base(context.Incidents)
        {
        }

        public Task<Incident?> GetByIdAsync(int id, CancellationToken ct)
        {
            return DbSet.FirstOrDefaultAsync(i => i.IncidentId == id, ct);
        }

        public void Add(Incident incident)
        {
            DbSet.Add(incident);
        }

        public void Update(Incident incident)
        {
            DbSet.Update(incident);
        }

        public void Remove(Incident incident)
        {
            DbSet.Remove(incident);
        }
    }
}
