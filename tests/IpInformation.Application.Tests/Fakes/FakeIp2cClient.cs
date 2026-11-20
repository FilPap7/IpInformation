using IpInformation.Application.Abstractions.Ip2c;

namespace IpInformation.Application.Tests.Fakes;

/// <summary>
/// Answers from <see cref="Responses"/>; an IP that is not there is unknown to IP2C.
/// </summary>
internal sealed class FakeIp2cClient : IIp2cClient
{
    public Dictionary<string, Ip2cCountry> Responses { get; } = [];

    public int Calls { get; private set; }

    public Task<Ip2cCountry?> LookupAsync(string ip, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(Responses.GetValueOrDefault(ip));
    }
}
