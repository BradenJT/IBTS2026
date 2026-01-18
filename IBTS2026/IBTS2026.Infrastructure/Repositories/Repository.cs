using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories
{
    internal class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbSet<T> DbSet;

        public Repository(DbSet<T> dbSet)
        {
            DbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        }

        public IQueryable<T> Query() => DbSet.AsQueryable();

        public Task<T?> GetByIdAsync(
            object[] keyValues,
            CancellationToken ct = default)
            => DbSet.FindAsync(keyValues, ct).AsTask();

        public Task AddAsync(T entity, CancellationToken ct = default)
            => DbSet.AddAsync(entity, ct).AsTask();

        public void Update(T entity)
            => DbSet.Update(entity);

        public void Remove(T entity)
            => DbSet.Remove(entity);
    }

}
