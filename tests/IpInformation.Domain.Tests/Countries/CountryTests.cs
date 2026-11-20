using IpInformation.Domain.Countries;

namespace IpInformation.Domain.Tests.Countries;

public class CountryTests
{
    private static readonly DateTime UtcNow = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WhenValid_ReturnsCountryWithGivenValues()
    {
        var result = Country.Create("Greece", "GR", "GRC", UtcNow);

        Assert.True(result.IsSuccess);
        var country = result.Value;
        Assert.Equal("Greece", country.Name);
        Assert.Equal("GR", country.TwoLetterCode);
        Assert.Equal("GRC", country.ThreeLetterCode);
        Assert.Equal(UtcNow, country.CreatedAt);
        Assert.Equal(0, country.Id);
        Assert.Empty(country.DomainEvents);
    }

    [Fact]
    public void Create_WhenCodesAreLowerCase_StoresThemUpperCase()
    {
        var country = Country.Create("Greece", "gr", "grc", UtcNow).Value;

        Assert.Equal("GR", country.TwoLetterCode);
        Assert.Equal("GRC", country.ThreeLetterCode);
    }

    [Fact]
    public void Create_WhenNameHasMaxLength_ReturnsSuccess()
    {
        var name = new string('a', Country.NameMaxLength);

        var result = Country.Create(name, "GR", "GRC", UtcNow);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WhenNameIsLongIp2cName_ReturnsSuccess()
    {
        var result = Country.Create("United Kingdom of Great Britain and Northern Ireland (the)", "GB", "GBR", UtcNow);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsMissing_ReturnsInvalidName(string? name)
    {
        var result = Country.Create(name, "GR", "GRC", UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(CountryErrors.InvalidName, result.Error);
    }

    [Fact]
    public void Create_WhenNameIsTooLong_ReturnsInvalidName()
    {
        var name = new string('a', Country.NameMaxLength + 1);

        var result = Country.Create(name, "GR", "GRC", UtcNow);

        Assert.Equal(CountryErrors.InvalidName, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("G")]
    [InlineData("GRC")]
    [InlineData("G1")]
    [InlineData("G ")]
    [InlineData("ΕΛ")]
    public void Create_WhenTwoLetterCodeIsInvalid_ReturnsInvalidTwoLetterCode(string? code)
    {
        var result = Country.Create("Greece", code, "GRC", UtcNow);

        Assert.Equal(CountryErrors.InvalidTwoLetterCode, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("GR")]
    [InlineData("GREC")]
    [InlineData("IA ")]
    [InlineData("G1C")]
    [InlineData("ΕΛΛ")]
    public void Create_WhenThreeLetterCodeIsInvalid_ReturnsInvalidThreeLetterCode(string? code)
    {
        var result = Country.Create("Greece", "GR", code, UtcNow);

        Assert.Equal(CountryErrors.InvalidThreeLetterCode, result.Error);
    }
}
