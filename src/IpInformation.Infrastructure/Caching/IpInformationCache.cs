using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.IpAddresses;
using Microsoft.Extensions.Caching.Memory;

namespace IpInformation.Infrastructure.Caching;

/// <summary>
/// In-process cache of IP information. Entries do not expire: the hourly update job removes an IP
/// when its country changes. A size limit keeps memory bounded.
/// </summary>
/// <remarks>
/// Uses its own <see cref="MemoryCache"/> instead of the shared <see cref="IMemoryCache"/>,
/// so the size limit applies only to IP entries and nothing else can evict or overwrite them.
/// Registered as a singleton.
/// </remarks>
internal sealed class IpInformationCache : IIpInformationCache, IDisposable
{
    /// <summary>
    /// Maximum number of cached IPs. When reached, new IPs are not cached until the cache compacts
    /// (they are still served from the database).
    /// </summary>
    internal const long SizeLimit = 100_000;

    private static readonly MemoryCacheEntryOptions EntryOptions = new() { Size = 1 };

    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = SizeLimit });

    public Task<IpInformationDto?> GetAsync(string ip, CancellationToken cancellationToken) =>
        Task.FromResult(_cache.TryGetValue(ip, out IpInformationDto? information) ? information : null);

    public Task SetAsync(string ip, IpInformationDto information, CancellationToken cancellationToken)
    {
        _cache.Set(ip, information, EntryOptions);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string ip, CancellationToken cancellationToken)
    {
        _cache.Remove(ip);
        return Task.CompletedTask;
    }

    public void Dispose() => _cache.Dispose();
}
