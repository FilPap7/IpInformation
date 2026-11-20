using IpInformation.Application.IpAddresses;
using IpInformation.Infrastructure.Caching;

namespace IpInformation.IntegrationTests.Caching;

public sealed class IpInformationCacheTests : IDisposable
{
    private static readonly IpInformationDto UnitedStates = new("United States", "US", "USA");

    private readonly IpInformationCache _cache = new();

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task GetAsync_WhenIpIsNotCached_ReturnsNull()
    {
        Assert.Null(await _cache.GetAsync("8.8.8.8", CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_AfterSetAsync_ReturnsTheInformation()
    {
        await _cache.SetAsync("8.8.8.8", UnitedStates, CancellationToken.None);

        Assert.Equal(UnitedStates, await _cache.GetAsync("8.8.8.8", CancellationToken.None));
    }

    [Fact]
    public async Task SetAsync_WhenIpIsAlreadyCached_ReplacesTheInformation()
    {
        var canada = new IpInformationDto("Canada", "CA", "CAN");
        await _cache.SetAsync("8.8.8.8", UnitedStates, CancellationToken.None);

        await _cache.SetAsync("8.8.8.8", canada, CancellationToken.None);

        Assert.Equal(canada, await _cache.GetAsync("8.8.8.8", CancellationToken.None));
    }

    [Fact]
    public async Task RemoveAsync_WhenIpIsCached_RemovesOnlyThatIp()
    {
        await _cache.SetAsync("8.8.8.8", UnitedStates, CancellationToken.None);
        await _cache.SetAsync("8.8.4.4", UnitedStates, CancellationToken.None);

        await _cache.RemoveAsync("8.8.8.8", CancellationToken.None);

        Assert.Null(await _cache.GetAsync("8.8.8.8", CancellationToken.None));
        Assert.Equal(UnitedStates, await _cache.GetAsync("8.8.4.4", CancellationToken.None));
    }

    [Fact]
    public async Task RemoveAsync_WhenIpIsNotCached_DoesNothing()
    {
        await _cache.RemoveAsync("8.8.8.8", CancellationToken.None);

        Assert.Null(await _cache.GetAsync("8.8.8.8", CancellationToken.None));
    }
}
