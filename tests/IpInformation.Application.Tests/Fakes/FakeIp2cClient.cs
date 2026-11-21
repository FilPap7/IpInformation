using IpInformation.Application.Abstractions.Ip2c;

namespace IpInformation.Application.Tests.Fakes;

/// <summary>
/// Answers from <see cref="Responses"/>; an IP that is not there is unknown to IP2C.
/// An IP in <see cref="Failing"/> throws, like IP2C being unreachable.
/// </summary>
internal sealed class FakeIp2cClient : IIp2cClient
{
    public Dictionary<string, Ip2cCountry> Responses { get; } = [];

    public HashSet<string> Failing { get; } = [];

    public int Calls { get; private set; }

    public Task<Ip2cCountry?> LookupAsync(string ip, CancellationToken cancellationToken)
    {
        Calls++;

        if (Failing.Contains(ip))
        {
            throw new HttpRequestException("IP2C is unreachable.");
        }

        return Task.FromResult(Responses.GetValueOrDefault(ip));
    }
}
