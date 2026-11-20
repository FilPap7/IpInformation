namespace IpInformation.Domain.Abstractions;

/// <summary>
/// The category of an expected failure. The API maps each category to an HTTP status (ProblemDetails).
/// </summary>
public enum ErrorType
{
    None = 0,
    Failure,
    Validation,
    NotFound,
    Conflict,
}

/// <summary>
/// An expected failure, e.g. an invalid IP address. <see cref="Code"/> is stable and machine-readable
/// (e.g. <c>IpAddress.Invalid</c>); <see cref="Description"/> is for humans.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
