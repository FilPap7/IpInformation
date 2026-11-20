using IpInformation.Application.Countries.Queries.GetCountryReport;
using IpInformation.Domain.Countries;

namespace IpInformation.Application.Tests.Countries.Queries.GetCountryReport;

public class GetCountryReportValidatorTests
{
    private readonly GetCountryReportValidator _validator = new();

    [Fact]
    public void Validate_WhenCodesAreNull_Passes()
    {
        Assert.True(_validator.Validate(new GetCountryReportQuery(null)).IsValid);
    }

    [Fact]
    public void Validate_WhenCodesAreEmpty_Passes()
    {
        Assert.True(_validator.Validate(new GetCountryReportQuery([])).IsValid);
    }

    [Fact]
    public void Validate_WhenCodesAreValid_Passes()
    {
        Assert.True(_validator.Validate(new GetCountryReportQuery(["GR", "us", "Zz"])).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("G")]
    [InlineData("GRC")]
    [InlineData("G1")]
    [InlineData("Γρ")]
    public void Validate_WhenACodeIsInvalid_FailsWithInvalidTwoLetterCode(string code)
    {
        var result = _validator.Validate(new GetCountryReportQuery(["GR", code]));

        var error = Assert.Single(result.Errors);
        Assert.Equal(CountryErrors.InvalidTwoLetterCode.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenACodeIsNull_FailsWithInvalidTwoLetterCode()
    {
        var result = _validator.Validate(new GetCountryReportQuery(["GR", null!]));

        var error = Assert.Single(result.Errors);
        Assert.Equal(CountryErrors.InvalidTwoLetterCode.Code, error.ErrorCode);
    }
}
