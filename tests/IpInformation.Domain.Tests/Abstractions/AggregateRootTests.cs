namespace IpInformation.Domain.Tests.Abstractions;

public class AggregateRootTests
{
    [Fact]
    public void DomainEvents_WhenNewAggregate_IsEmpty()
    {
        var aggregate = new TestAggregate();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_WhenCalled_AddsEventsInOrder()
    {
        var aggregate = new TestAggregate();

        aggregate.DoSomething("first");
        aggregate.DoSomething("second");

        Assert.Equal(
            [new TestDomainEvent("first"), new TestDomainEvent("second")],
            aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_WhenEventsRaised_RemovesAllEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.DoSomething("first");

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }
}
