using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories
{
    internal class RepositoryBase<T> where T : class
    {
        protected readonly DbSet<T> DbSet;

        public RepositoryBase(DbSet<T> dbSet)
        {
            DbSet = dbSet ?? throw new ArgumentNullException(nameof(dbSet));
        }

        protected IQueryable<T> Query() => DbSet.AsQueryable();
        protected void AddEntity(T entity) => DbSet.Add(entity);
        protected void UpdateEntity(T entity) => DbSet.Update(entity);
        protected void RemoveEntity(T entity) => DbSet.Remove(entity);
    }

}
