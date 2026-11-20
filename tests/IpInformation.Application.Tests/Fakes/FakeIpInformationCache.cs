using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.IpAddresses;

namespace IpInformation.Application.Tests.Fakes;

internal sealed class FakeIpInformationCache : IIpInformationCache
{
    public Dictionary<string, IpInformationDto> Entries { get; } = [];

    public Task<IpInformationDto?> GetAsync(string ip, CancellationToken cancellationToken) =>
        Task.FromResult(Entries.GetValueOrDefault(ip));

    public Task SetAsync(string ip, IpInformationDto information, CancellationToken cancellationToken)
    {
        Entries[ip] = information;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string ip, CancellationToken cancellationToken)
    {
        Entries.Remove(ip);
        return Task.CompletedTask;
    }
}
