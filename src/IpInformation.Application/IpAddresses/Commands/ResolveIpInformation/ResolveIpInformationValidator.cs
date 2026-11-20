using FluentValidation;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;

/// <summary>
/// Rejects invalid IPs before any cache, database or IP2C call. Reuses the domain rule.
/// </summary>
public sealed class ResolveIpInformationValidator : AbstractValidator<ResolveIpInformationCommand>
{
    public ResolveIpInformationValidator()
    {
        RuleFor(c => c.Ip)
            .Must(IpAddress.IsValidIpV4)
            .WithErrorCode(IpAddressErrors.InvalidIp.Code)
            .WithMessage(IpAddressErrors.InvalidIp.Description);
    }
}
