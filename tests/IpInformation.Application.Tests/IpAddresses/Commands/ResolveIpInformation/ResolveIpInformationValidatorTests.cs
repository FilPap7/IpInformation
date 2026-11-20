using IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.Tests.IpAddresses.Commands.ResolveIpInformation;

public class ResolveIpInformationValidatorTests
{
    private readonly ResolveIpInformationValidator _validator = new();

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("10.0.0.1")]
    public void Validate_WhenIpIsValidIpV4_Passes(string ip)
    {
        var result = _validator.Validate(new ResolveIpInformationCommand(ip));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1")]
    [InlineData("256.1.1.1")]
    [InlineData("::1")]
    public void Validate_WhenIpIsNotValidIpV4_FailsWithInvalidIp(string ip)
    {
        var result = _validator.Validate(new ResolveIpInformationCommand(ip));

        var error = Assert.Single(result.Errors);
        Assert.Equal(IpAddressErrors.InvalidIp.Code, error.ErrorCode);
        Assert.Equal(IpAddressErrors.InvalidIp.Description, error.ErrorMessage);
    }
}
