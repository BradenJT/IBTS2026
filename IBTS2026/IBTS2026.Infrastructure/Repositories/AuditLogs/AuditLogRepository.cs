using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.AuditLogs;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.AuditLogs
{
    internal sealed class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(IBTS2026Context context)
            : base(context.AuditLogs)
        {
        }

        public void Add(AuditLog auditLog) => AddEntity(auditLog);

        public Task<AuditLog?> GetByIdAsync(int id, CancellationToken ct)
            => Query().FirstOrDefaultAsync(a => a.AuditLogId == id, ct);
    }
}
