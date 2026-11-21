using IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;

namespace IpInformation.Application.Tests.IpAddresses.Commands.RefreshIpAddressBatch;

public class RefreshIpAddressBatchValidatorTests
{
    private readonly RefreshIpAddressBatchValidator _validator = new();

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 100)]
    public void Validate_WhenValuesAreInRange_Passes(int afterId, int batchSize)
    {
        Assert.True(_validator.Validate(new RefreshIpAddressBatchCommand(afterId, batchSize)).IsValid);
    }

    [Fact]
    public void Validate_WhenAfterIdIsNegative_FailsWithInvalidAfterId()
    {
        var error = Assert.Single(_validator.Validate(new RefreshIpAddressBatchCommand(-1, 100)).Errors);

        Assert.Equal("RefreshIpAddressBatch.InvalidAfterId", error.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WhenBatchSizeIsNotPositive_FailsWithInvalidBatchSize(int batchSize)
    {
        var error = Assert.Single(_validator.Validate(new RefreshIpAddressBatchCommand(0, batchSize)).Errors);

        Assert.Equal("RefreshIpAddressBatch.InvalidBatchSize", error.ErrorCode);
    }
}
