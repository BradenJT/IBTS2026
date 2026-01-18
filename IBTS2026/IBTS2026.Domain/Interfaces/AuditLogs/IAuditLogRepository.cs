using IBTS2026.Domain.Entities;

namespace IBTS2026.Domain.Interfaces.AuditLogs
{
    public interface IAuditLogRepository
    {
        Task<AuditLog?> GetByIdAsync(int id, CancellationToken ct);
        void Add(AuditLog auditLog);
    }
}
