using IpInformation.Domain.Abstractions;

namespace IpInformation.Infrastructure.Events;

/// <summary>
/// Sends domain events to their handlers. Called by the DbContext after SaveChanges succeeds.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
