using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.Abstractions.Events;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.IpAddresses.EventHandlers;

/// <summary>
/// Removes the cached information of an IP whose country changed, so the next lookup reads the new country
/// from the database and caches it again.
/// </summary>
public sealed class IpAddressCountryChangedHandler(IIpInformationCache cache)
    : IDomainEventHandler<IpAddressCountryChanged>
{
    public Task HandleAsync(IpAddressCountryChanged domainEvent, CancellationToken cancellationToken) =>
        cache.RemoveAsync(domainEvent.Ip, cancellationToken);
}
