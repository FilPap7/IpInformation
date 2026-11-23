using IpInformation.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IpInformation.Application.Behaviors;

/// <summary>
/// Logs every request: its name, how long it took and, on an expected failure, the error code.
/// Registered first, so the time includes validation and a validation failure is logged too.
/// </summary>
/// <remarks>
/// Request contents are not logged (they may hold personal data such as IPs).
/// Exceptions are not logged here: they propagate to the API exception handler or the job, which log them once.
/// The correlation ID is not passed in: it comes from the logging scope of the request or job cycle.
/// </remarks>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    TimeProvider timeProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly string RequestName = typeof(TRequest).Name;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetTimestamp();

        var response = await next(cancellationToken);

        var elapsedMs = (long)timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;

        if (response.IsSuccess)
        {
            logger.LogInformation("{RequestName} succeeded in {ElapsedMs} ms", RequestName, elapsedMs);
        }
        else
        {
            logger.LogWarning(
                "{RequestName} failed with {ErrorCode} in {ElapsedMs} ms: {ErrorDescription}",
                RequestName,
                response.Error.Code,
                elapsedMs,
                response.Error.Description);
        }

        return response;
    }
}
