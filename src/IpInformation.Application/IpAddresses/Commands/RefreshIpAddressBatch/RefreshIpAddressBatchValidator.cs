using FluentValidation;

namespace IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;

public sealed class RefreshIpAddressBatchValidator : AbstractValidator<RefreshIpAddressBatchCommand>
{
    public RefreshIpAddressBatchValidator()
    {
        RuleFor(c => c.AfterId)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode("RefreshIpAddressBatch.InvalidAfterId");

        RuleFor(c => c.BatchSize)
            .GreaterThan(0)
            .WithErrorCode("RefreshIpAddressBatch.InvalidBatchSize");
    }
}
