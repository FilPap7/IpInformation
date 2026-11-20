using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.Countries;

/// <summary>
/// A country as returned by IP2C. Codes are stored upper-case.
/// </summary>
public sealed class Country : AggregateRoot
{
    public const int NameMaxLength = 100;
    public const int TwoLetterCodeLength = 2;
    public const int ThreeLetterCodeLength = 3;

    private Country(string name, string twoLetterCode, string threeLetterCode, DateTime createdAt)
    {
        Name = name;
        TwoLetterCode = twoLetterCode;
        ThreeLetterCode = threeLetterCode;
        CreatedAt = createdAt;
    }

    public string Name { get; private set; }

    public string TwoLetterCode { get; private set; }

    public string ThreeLetterCode { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<Country> Create(string? name, string? twoLetterCode, string? threeLetterCode, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > NameMaxLength)
        {
            return Result.Failure<Country>(CountryErrors.InvalidName);
        }

        if (!IsLatinLetters(twoLetterCode, TwoLetterCodeLength))
        {
            return Result.Failure<Country>(CountryErrors.InvalidTwoLetterCode);
        }

        if (!IsLatinLetters(threeLetterCode, ThreeLetterCodeLength))
        {
            return Result.Failure<Country>(CountryErrors.InvalidThreeLetterCode);
        }

        return Result.Success(new Country(
            name,
            twoLetterCode!.ToUpperInvariant(),
            threeLetterCode!.ToUpperInvariant(),
            utcNow));
    }

    private static bool IsLatinLetters(string? value, int length) =>
        value is not null && value.Length == length && value.All(char.IsAsciiLetter);
}

public static class CountryErrors
{
    public static readonly Error InvalidName = Error.Validation(
        "Country.InvalidName",
        $"A country name is required and must be at most {Country.NameMaxLength} characters.");

    public static readonly Error InvalidTwoLetterCode = Error.Validation(
        "Country.InvalidTwoLetterCode",
        $"A two-letter country code must consist of exactly {Country.TwoLetterCodeLength} Latin letters.");

    public static readonly Error InvalidThreeLetterCode = Error.Validation(
        "Country.InvalidThreeLetterCode",
        $"A three-letter country code must consist of exactly {Country.ThreeLetterCodeLength} Latin letters.");
}
