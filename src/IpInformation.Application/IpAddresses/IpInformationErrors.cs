using IpInformation.Domain.Abstractions;

namespace IpInformation.Application.IpAddresses;

public static class IpInformationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "IpInformation.NotFound",
        "No information was found for this IP.");
}
