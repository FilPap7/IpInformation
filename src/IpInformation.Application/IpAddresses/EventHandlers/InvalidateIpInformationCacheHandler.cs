using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.Abstractions.Events;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.IpAddresses.EventHandlers;

/// <summary>
/// Removes the cached information of an IP whose country changed or that was removed, so the next lookup
/// reads it from the database (or IP2C) again instead of serving stale data from the cache.
/// </summary>
public sealed class InvalidateIpInformationCacheHandler(IIpInformationCache cache)
    : IDomainEventHandler<IpAddressCountryChanged>,
      IDomainEventHandler<IpAddressRemoved>
{
    public Task HandleAsync(IpAddressCountryChanged domainEvent, CancellationToken cancellationToken) =>
        cache.RemoveAsync(domainEvent.Ip, cancellationToken);

    public Task HandleAsync(IpAddressRemoved domainEvent, CancellationToken cancellationToken) =>
        cache.RemoveAsync(domainEvent.Ip, cancellationToken);
}
