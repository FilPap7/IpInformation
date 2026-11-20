using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;

/// <summary>
/// Returns the country of an IP: from the cache, else the database, else IP2C (then stored in both).
/// A command, not a query, because an IP2C lookup persists a new IP.
/// </summary>
public sealed record ResolveIpInformationCommand(string Ip) : IRequest<Result<IpInformationDto>>;
