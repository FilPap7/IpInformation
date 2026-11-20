using IpInformation.Application.IpAddresses;

namespace IpInformation.Application.Abstractions.Caching;

/// <summary>
/// Cache of IP information, keyed by IP.
/// </summary>
public interface IIpInformationCache
{
    Task<IpInformationDto?> GetAsync(string ip, CancellationToken cancellationToken);

    Task SetAsync(string ip, IpInformationDto information, CancellationToken cancellationToken);

    Task RemoveAsync(string ip, CancellationToken cancellationToken);
}
