namespace IBTS2026.Infrastructure.Repositories
{
    internal interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<T> GetByIdAsync(
            object[] keyValues,
            CancellationToken ct = default
        );
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
    }
}
