using IpInformation.Domain.Abstractions;

namespace IpInformation.Domain.IpAddresses;

/// <summary>
/// Raised when an IP address is removed. Used to invalidate the cached information of that IP.
/// </summary>
public sealed record IpAddressRemoved(string Ip) : IDomainEvent;
