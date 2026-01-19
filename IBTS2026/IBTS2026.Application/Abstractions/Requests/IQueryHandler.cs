namespace IBTS2026.Application.Abstractions.Requests
{
    public interface IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(TQuery query, CancellationToken ct);
    }
}
