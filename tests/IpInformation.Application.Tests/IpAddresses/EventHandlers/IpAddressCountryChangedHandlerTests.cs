using IpInformation.Application.Abstractions.Events;
using IpInformation.Application.IpAddresses;
using IpInformation.Application.IpAddresses.EventHandlers;
using IpInformation.Application.Tests.Fakes;
using IpInformation.Domain.IpAddresses;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Application.Tests.IpAddresses.EventHandlers;

public class IpAddressCountryChangedHandlerTests
{
    private static readonly IpInformationDto UnitedStates = new("United States", "US", "USA");

    [Fact]
    public async Task HandleAsync_WhenTheIpIsCached_RemovesOnlyThatIp()
    {
        var cache = new FakeIpInformationCache();
        cache.Entries["8.8.8.8"] = UnitedStates;
        cache.Entries["1.1.1.1"] = UnitedStates;
        var handler = new IpAddressCountryChangedHandler(cache);

        await handler.HandleAsync(new IpAddressCountryChanged("8.8.8.8"), CancellationToken.None);

        Assert.Equal(["1.1.1.1"], cache.Entries.Keys);
    }

    [Fact]
    public async Task HandleAsync_WhenTheIpIsNotCached_DoesNothing()
    {
        var cache = new FakeIpInformationCache();
        var handler = new IpAddressCountryChangedHandler(cache);

        await handler.HandleAsync(new IpAddressCountryChanged("8.8.8.8"), CancellationToken.None);

        Assert.Empty(cache.Entries);
    }

    [Fact]
    public void AddApplication_Always_RegistersTheHandlerForItsEvent()
    {
        var services = new ServiceCollection().AddApplication();

        var registration = Assert.Single(services, d => d.ServiceType == typeof(IDomainEventHandler<IpAddressCountryChanged>));
        Assert.Equal(typeof(IpAddressCountryChangedHandler), registration.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
