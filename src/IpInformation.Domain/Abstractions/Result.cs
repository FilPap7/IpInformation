namespace IpInformation.Domain.Abstractions;

/// <summary>
/// The outcome of an operation that can fail in an expected way: either success, or failure with an <see cref="Error"/>.
/// Used instead of exceptions for expected failures (validation, not found, rule violation).
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("A successful result cannot have an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("A failed result must have an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The value of a successful result. Accessing it on a failed result is a programming error.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");
}
