namespace IBTS2026.Application.Abstractions.Requests
{
    public interface IRequestHandler<TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct);
    }
}
