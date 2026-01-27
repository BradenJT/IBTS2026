using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Application.Abstractions.Requests
{
    public interface IRequestDispatcher
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct);
    }

    public sealed class RequestDispatcher : IRequestDispatcher
    {
        private static readonly ActivitySource ActivitySource = new("IBTS2026.Application");
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequestDispatcher> _logger;

        public RequestDispatcher(IServiceProvider serviceProvider, ILogger<RequestDispatcher> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken ct)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestTypeName = typeof(TRequest).Name;
            using var activity = ActivitySource.StartActivity($"Handle {requestTypeName}");
            activity?.SetTag("request.type", requestTypeName);

            var stopwatch = Stopwatch.StartNew();

            _logger.LogDebug("Handling request {RequestType}", requestTypeName);

            try
            {
                var handler = _serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>))
                              as IRequestHandler<TRequest, TResponse>;

                if (handler == null)
                {
                    _logger.LogError("No handler found for request type {RequestType}", requestTypeName);
                    throw new InvalidOperationException($"No handler found for request of type {typeof(TRequest).FullName}");
                }

                var result = await handler.Handle(request, ct);

                stopwatch.Stop();
                activity?.SetTag("request.success", true);
                activity?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);

                _logger.LogInformation(
                    "Handled {RequestType} successfully in {ElapsedMs}ms",
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                activity?.SetTag("request.success", false);
                activity?.SetTag("request.error", ex.Message);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                _logger.LogError(
                    ex,
                    "Failed to handle {RequestType} after {ElapsedMs}ms",
                    requestTypeName,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
