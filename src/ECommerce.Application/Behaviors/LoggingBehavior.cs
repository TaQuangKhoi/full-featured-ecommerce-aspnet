using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs every request and response with execution time.
/// Follows Open/Closed Principle — no handler modification required.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "[MediatR] Handling {RequestName} | Payload: {@Request}",
            requestName, request);

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "[MediatR] Handled {RequestName} in {ElapsedMs}ms | Response: {@Response}",
            requestName, stopwatch.ElapsedMilliseconds, response);

        return response;
    }
}
