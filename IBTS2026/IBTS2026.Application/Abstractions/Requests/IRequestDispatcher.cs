namespace IBTS2026.Application.Abstractions.Requests
{
    public interface IRequestDispatcher
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct);
    }

    public sealed class RequestDispatcher : IRequestDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        public RequestDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            var handler = _serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>)) 
                          as IRequestHandler<TRequest, TResponse>;
            if (handler == null)
            {
                throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).FullName}");
            }
            return await handler.Handle(request, ct);
        }
    }
}
