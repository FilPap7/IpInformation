namespace IpInformation.Application.Abstractions.Ip2c;

/// <summary>
/// Client of the IP2C web service (https://ip2c.org).
/// </summary>
public interface IIp2cClient
{
    /// <summary>
    /// Looks up the country of an IPv4 address.
    /// Throws when IP2C is unavailable or its response cannot be read (unexpected failure).
    /// </summary>
    /// <returns>The country, or <c>null</c> when IP2C does not know the IP.</returns>
    Task<Ip2cCountry?> LookupAsync(string ip, CancellationToken cancellationToken);
}

public sealed record Ip2cCountry(string Name, string TwoLetterCode, string ThreeLetterCode);
