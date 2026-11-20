using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IpInformation.Api.ExceptionHandlers;

/// <summary>
/// Turns a failed or timed-out IP2C call into 503 Service Unavailable: the caller may retry later.
/// Any other exception is left to the default handler (500).
/// </summary>
internal sealed class Ip2cUnavailableExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<Ip2cUnavailableExceptionHandler> logger)
    : IExceptionHandler
{
    public const string ErrorCode = "Ip2c.Unavailable";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!IsIp2cFailure(exception))
        {
            return false;
        }

        logger.LogWarning(exception, "IP2C is unavailable.");

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Detail = "The IP information service is temporarily unavailable. Try again later.",
                Extensions = { ["code"] = ErrorCode },
            },
        });
    }

    // IP2C is the only HTTP dependency, so every HttpClient failure comes from it.
    // HttpClient reports its timeout as a TaskCanceledException wrapping a TimeoutException.
    internal static bool IsIp2cFailure(Exception exception) =>
        exception is HttpRequestException or TaskCanceledException { InnerException: TimeoutException };
}
