namespace IpInformation.Infrastructure.Ip2c;

/// <summary>
/// Settings of the IP2C client, bound from the <c>Ip2c</c> configuration section.
/// </summary>
public sealed class Ip2cOptions
{
    public const string SectionName = "Ip2c";

    /// <summary>
    /// Base address of the service, ending with a slash, e.g. <c>https://ip2c.org/</c>.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    /// Timeout of a single lookup.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}
