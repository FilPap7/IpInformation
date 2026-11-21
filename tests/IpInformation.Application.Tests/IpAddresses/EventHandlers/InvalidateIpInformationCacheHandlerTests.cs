using IpInformation.Application.Abstractions.Events;
using IpInformation.Application.IpAddresses;
using IpInformation.Application.IpAddresses.EventHandlers;
using IpInformation.Application.Tests.Fakes;
using IpInformation.Domain.IpAddresses;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Application.Tests.IpAddresses.EventHandlers;

public class InvalidateIpInformationCacheHandlerTests
{
    private static readonly IpInformationDto UnitedStates = new("United States", "US", "USA");

    private readonly FakeIpInformationCache _cache = new();
    private readonly InvalidateIpInformationCacheHandler _handler;

    public InvalidateIpInformationCacheHandlerTests()
    {
        _cache.Entries["8.8.8.8"] = UnitedStates;
        _cache.Entries["1.1.1.1"] = UnitedStates;
        _handler = new InvalidateIpInformationCacheHandler(_cache);
    }

    [Fact]
    public async Task HandleAsync_WhenTheCountryOfACachedIpChanged_RemovesOnlyThatIp()
    {
        await _handler.HandleAsync(new IpAddressCountryChanged("8.8.8.8"), CancellationToken.None);

        Assert.Equal(["1.1.1.1"], _cache.Entries.Keys);
    }

    [Fact]
    public async Task HandleAsync_WhenACachedIpWasRemoved_RemovesOnlyThatIp()
    {
        await _handler.HandleAsync(new IpAddressRemoved("8.8.8.8"), CancellationToken.None);

        Assert.Equal(["1.1.1.1"], _cache.Entries.Keys);
    }

    [Fact]
    public async Task HandleAsync_WhenTheIpIsNotCached_LeavesTheCacheUnchanged()
    {
        await _handler.HandleAsync(new IpAddressCountryChanged("9.9.9.9"), CancellationToken.None);

        Assert.Equal(2, _cache.Entries.Count);
    }

    [Theory]
    [InlineData(typeof(IDomainEventHandler<IpAddressCountryChanged>))]
    [InlineData(typeof(IDomainEventHandler<IpAddressRemoved>))]
    public void AddApplication_Always_RegistersTheHandlerForEachOfItsEvents(Type serviceType)
    {
        var services = new ServiceCollection().AddApplication();

        var registration = Assert.Single(services, d => d.ServiceType == serviceType);
        Assert.Equal(typeof(InvalidateIpInformationCacheHandler), registration.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
