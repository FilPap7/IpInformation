using System.Collections.Concurrent;
using System.Reflection;
using IpInformation.Application.Abstractions.Events;
using IpInformation.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.Infrastructure.Events;

/// <summary>
/// Calls every <see cref="IDomainEventHandler{TDomainEvent}"/> registered for each event, in order, one at a time.
/// </summary>
/// <remarks>
/// Handlers are resolved when an event is dispatched, not injected, because a handler may depend on a repository,
/// which depends on the DbContext, which depends on this dispatcher: injecting them would be a cycle.
/// Scoped, so handlers share the request's scope (and DbContext).
/// </remarks>
internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private delegate Task DispatchDelegate(IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken);

    private static readonly MethodInfo DispatchTypedMethod =
        typeof(DomainEventDispatcher).GetMethod(nameof(DispatchTypedAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    // One compiled delegate per event type, so reflection runs once per type instead of once per event.
    private static readonly ConcurrentDictionary<Type, DispatchDelegate> Dispatchers = new();

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var dispatch = Dispatchers.GetOrAdd(
                domainEvent.GetType(),
                eventType => DispatchTypedMethod.MakeGenericMethod(eventType).CreateDelegate<DispatchDelegate>());

            await dispatch(serviceProvider, domainEvent, cancellationToken);
        }
    }

    private static async Task DispatchTypedAsync<TDomainEvent>(
        IServiceProvider serviceProvider, IDomainEvent domainEvent, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>())
        {
            await handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken);
        }
    }
}
