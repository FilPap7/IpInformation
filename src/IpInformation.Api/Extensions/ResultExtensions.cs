using IpInformation.Domain.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IpInformation.Api.Extensions;

internal static class ResultExtensions
{
    /// <summary>
    /// Maps a failed <see cref="Result"/> to a ProblemDetails response (RFC 9457).
    /// The error code is added as the <c>code</c> extension so that clients can check it in code.
    /// </summary>
    public static ProblemHttpResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be mapped to a problem.");
        }

        var error = result.Error;

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
