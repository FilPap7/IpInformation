using Microsoft.Extensions.Primitives;

namespace IpInformation.Api.Middleware;

/// <summary>
/// Gives every request a correlation ID: the caller's <c>X-Correlation-ID</c> header when it is safe,
/// otherwise a new one. The ID is returned in the response header, added to error responses,
/// and put in a logging scope so every log line written during the request carries it.
/// </summary>
internal sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";

    private const int MaxLength = 64;
    private static readonly object ItemKey = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = SafeValueOrNull(context.Request.Headers[HeaderName]) is { } provided
            ? provided
            : Guid.NewGuid().ToString("N");

        context.Items[ItemKey] = correlationId;

        // Set when the response starts, so it survives handlers that clear the response (e.g. the exception handler).
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
        {
            await next(context);
        }
    }

    /// <summary>
    /// The correlation ID of the current request, or <c>null</c> outside a request handled by this middleware.
    /// </summary>
    public static string? Get(HttpContext context) => context.Items[ItemKey] as string;

    /// <summary>
    /// Adds the correlation ID to every ProblemDetails response, so a caller can quote it when reporting an error.
    /// </summary>
    public static void AddToProblemDetails(ProblemDetailsContext context)
    {
        if (Get(context.HttpContext) is { } correlationId)
        {
            context.ProblemDetails.Extensions["correlationId"] = correlationId;
        }
    }

    // A caller-supplied ID ends up in logs, so only short IDs of letters, digits, '-', '_' and '.' are accepted.
    private static string? SafeValueOrNull(StringValues header)
    {
        if (header.Count != 1 || header[0] is not { Length: > 0 and <= MaxLength } value)
        {
            return null;
        }

        return value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.') ? value : null;
    }
}
