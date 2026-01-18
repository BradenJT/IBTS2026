using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces;
using IBTS2026.Infrastructure.Repositories;

namespace IBTS2026.Infrastructure.Persistence
{
    public class UnitOfWork(
        IBTS2026Context dbContext) : IUnitOfWork
    {
        private readonly IBTS2026Context _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
