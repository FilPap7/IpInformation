namespace IpInformation.Infrastructure.BackgroundJobs;

/// <summary>
/// Settings of the IP refresh job, bound from the <c>IpAddressRefresh</c> configuration section.
/// </summary>
public sealed class IpAddressRefreshOptions
{
    public const string SectionName = "IpAddressRefresh";

    /// <summary>
    /// Time between the start of two refresh cycles. The first cycle runs at startup.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// How many IPs are loaded, refreshed and saved together.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
