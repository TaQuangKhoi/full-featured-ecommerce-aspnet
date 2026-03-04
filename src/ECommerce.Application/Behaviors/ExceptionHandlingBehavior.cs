using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that catches, logs, and re-throws exceptions from any handler.
/// Centralizes exception logging — Single Responsibility Principle.
/// </summary>
public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            return await next();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "[MediatR] {RequestName} was cancelled.",
                requestName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[MediatR] Unhandled exception in {RequestName} | Message: {Message} | Payload: {@Request}",
                requestName, ex.Message, request);
            throw;
        }
    }
}
