using System.Globalization;
using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.IpAddresses;

/// <summary>
/// An IPv4 address and the country it belongs to. The country is referenced by ID only (separate aggregate).
/// </summary>
public sealed class IpAddress : AggregateRoot
{
    public const int IpMaxLength = 15;

    private IpAddress(string ip, int countryId, DateTime createdAt)
    {
        Ip = ip;
        CountryId = countryId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public string Ip { get; private set; }

    public int CountryId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the country of this IP last changed (or when it was created).
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <param name="ip">Strict dotted-decimal IPv4 (private ranges are accepted).</param>
    /// <param name="countryId">ID of an existing, saved country.</param>
    public static Result<IpAddress> Create(string? ip, int countryId, DateTime utcNow)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(countryId);

        return IsValidIpV4(ip)
            ? Result.Success(new IpAddress(ip!, countryId, utcNow))
            : Result.Failure<IpAddress>(IpAddressErrors.InvalidIp);
    }

    /// <summary>
    /// Moves this IP to another country. Does nothing if the country is the same.
    /// </summary>
    /// <returns><c>true</c> if the country changed.</returns>
    public bool ChangeCountry(int countryId, DateTime utcNow)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(countryId);

        if (countryId == CountryId)
        {
            return false;
        }

        CountryId = countryId;
        UpdatedAt = utcNow;
        RaiseDomainEvent(new IpAddressCountryChanged(Ip));
        return true;
    }

    // Four numbers 0-255 separated by dots; no signs, whitespace or leading zeros ("01" can mean octal).
    // IPAddress.TryParse is not used because it also accepts forms like "1" (= 0.0.0.1).
    private static bool IsValidIpV4(string? ip)
    {
        if (ip is null || ip.Length > IpMaxLength)
        {
            return false;
        }

        var octets = ip.Split('.');
        return octets.Length == 4 && octets.All(octet =>
            octet.Length is >= 1 and <= 3
            && (octet.Length == 1 || octet[0] != '0')
            && byte.TryParse(octet, NumberStyles.None, CultureInfo.InvariantCulture, out _));
    }
}

public static class IpAddressErrors
{
    public static readonly Error InvalidIp = Error.Validation(
        "IpAddress.InvalidIp",
        "The IP must be a valid IPv4 address in dotted-decimal form, e.g. 44.255.255.254.");
}
