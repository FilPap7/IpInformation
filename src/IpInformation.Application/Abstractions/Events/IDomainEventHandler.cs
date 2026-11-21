using IpInformation.Domain.Abstractions;

namespace IpInformation.Application.Abstractions.Events;

/// <summary>
/// Reacts to a domain event. Called by Infrastructure after the change that raised the event has been saved.
/// </summary>
/// <remarks>
/// Every implementation in the Application assembly is registered automatically by <c>AddApplication</c>.
/// A handler runs after the save, so it must not assume it can undo the change; keep it idempotent.
/// </remarks>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
