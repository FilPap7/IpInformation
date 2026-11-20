using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.Tests.Abstractions;

internal sealed record TestDomainEvent(string Name) : IDomainEvent;

internal sealed class TestAggregate : AggregateRoot
{
    public void DoSomething(string name) => RaiseDomainEvent(new TestDomainEvent(name));
}
