using System.Reflection;
using FluentValidation;
using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.Behaviors;

/// <summary>
/// Runs every FluentValidation validator of the request before its handler.
/// On failure it returns a failed <see cref="Result"/> with <see cref="ErrorType.Validation"/> and the handler is not called.
/// Applies only to requests whose response is a <see cref="Result"/>.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly Func<Error, TResponse> CreateFailure = BuildFailureFactory();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Error holds a single error, so the first failure is reported.
        var failure = results.SelectMany(r => r.Errors).FirstOrDefault();

        if (failure is null)
        {
            return await next(cancellationToken);
        }

        return CreateFailure(Error.Validation(failure.ErrorCode, failure.ErrorMessage));
    }

    /// <summary>
    /// Builds, once per response type, a function that creates a failed <typeparamref name="TResponse"/>:
    /// <c>Result.Failure(error)</c> for <see cref="Result"/>, <c>Result.Failure&lt;T&gt;(error)</c> for <see cref="Result{TValue}"/>.
    /// </summary>
    private static Func<Error, TResponse> BuildFailureFactory()
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return error => (TResponse)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failure = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
                .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);

            return error => (TResponse)failure.Invoke(null, [error])!;
        }

        throw new InvalidOperationException($"{typeof(TResponse)} is not a supported result type.");
    }
}
