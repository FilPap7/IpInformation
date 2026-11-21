using IpInformation.Application.Abstractions.Events;
using IpInformation.Domain.Abstractions;
using IpInformation.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.IntegrationTests.Events;

public class DomainEventDispatcherTests
{
    private sealed record FirstEvent(string Value) : IDomainEvent;

    private sealed record SecondEvent(string Value) : IDomainEvent;

    private sealed class Recorder
    {
        public List<string> Calls { get; } = [];
    }

    private sealed class FirstHandlerA(Recorder recorder) : IDomainEventHandler<FirstEvent>
    {
        public Task HandleAsync(FirstEvent domainEvent, CancellationToken cancellationToken)
        {
            recorder.Calls.Add($"A:{domainEvent.Value}");
            return Task.CompletedTask;
        }
    }

    private sealed class FirstHandlerB(Recorder recorder) : IDomainEventHandler<FirstEvent>
    {
        public Task HandleAsync(FirstEvent domainEvent, CancellationToken cancellationToken)
        {
            recorder.Calls.Add($"B:{domainEvent.Value}");
            return Task.CompletedTask;
        }
    }

    private sealed class SecondHandler(Recorder recorder) : IDomainEventHandler<SecondEvent>
    {
        public Task HandleAsync(SecondEvent domainEvent, CancellationToken cancellationToken)
        {
            recorder.Calls.Add($"Second:{domainEvent.Value}");
            return Task.CompletedTask;
        }
    }

    private readonly Recorder _recorder = new();

    private DomainEventDispatcher CreateDispatcher(Action<IServiceCollection> registerHandlers)
    {
        var services = new ServiceCollection().AddSingleton(_recorder);
        registerHandlers(services);
        return new DomainEventDispatcher(services.BuildServiceProvider());
    }

    [Fact]
    public async Task DispatchAsync_WhenAnEventHasSeveralHandlers_CallsEachInRegistrationOrder()
    {
        var dispatcher = CreateDispatcher(services => services
            .AddScoped<IDomainEventHandler<FirstEvent>, FirstHandlerA>()
            .AddScoped<IDomainEventHandler<FirstEvent>, FirstHandlerB>());

        await dispatcher.DispatchAsync([new FirstEvent("1")], CancellationToken.None);

        Assert.Equal(["A:1", "B:1"], _recorder.Calls);
    }

    [Fact]
    public async Task DispatchAsync_WhenEventsHaveDifferentTypes_CallsOnlyTheHandlersOfEachType()
    {
        var dispatcher = CreateDispatcher(services => services
            .AddScoped<IDomainEventHandler<FirstEvent>, FirstHandlerA>()
            .AddScoped<IDomainEventHandler<SecondEvent>, SecondHandler>());

        await dispatcher.DispatchAsync(
            [new FirstEvent("1"), new SecondEvent("2"), new FirstEvent("3")],
            CancellationToken.None);

        Assert.Equal(["A:1", "Second:2", "A:3"], _recorder.Calls);
    }

    [Fact]
    public async Task DispatchAsync_WhenAnEventHasNoHandler_DoesNothing()
    {
        var dispatcher = CreateDispatcher(services => services
            .AddScoped<IDomainEventHandler<FirstEvent>, FirstHandlerA>());

        await dispatcher.DispatchAsync([new SecondEvent("2")], CancellationToken.None);

        Assert.Empty(_recorder.Calls);
    }
}
