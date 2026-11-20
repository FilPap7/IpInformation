namespace IpInformation.Domain.Abstractions;

/// <summary>
/// Marks something that happened in the domain (e.g. "the country of an IP address changed").
/// Kept free of MediatR so the Domain has no dependencies; Infrastructure dispatches events after SaveChanges.
/// </summary>
public interface IDomainEvent;
