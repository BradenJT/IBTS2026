namespace IBTS2026.Application.Abstractions.Requests
{
    internal interface IRequestHandler<TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct);
    }
}
