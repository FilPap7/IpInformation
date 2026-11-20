using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.IpAddresses;

/// <summary>
/// Raised when an IP address moves to another country. Used to invalidate the cached information of that IP.
/// </summary>
public sealed record IpAddressCountryChanged(string Ip) : IDomainEvent;
