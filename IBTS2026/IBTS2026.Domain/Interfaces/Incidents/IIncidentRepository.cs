using IBTS2026.Domain.Entities;

namespace IBTS2026.Domain.Interfaces.Incidents
{
    public interface IIncidentRepository
    {
        Task<Incident?> GetByIdAsync(int id, CancellationToken ct);
        void Add(Incident incident);
        void Update(Incident incident);
        void Remove(Incident incident);
    }
}
