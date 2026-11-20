using IpInformation.Domain.IpAddresses;

namespace IpInformation.Domain.Tests.IpAddresses;

public class IpAddressTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Later = CreatedAt.AddHours(1);

    [Fact]
    public void Create_WhenValid_ReturnsIpAddressWithGivenValues()
    {
        var result = IpAddress.Create("44.255.255.254", countryId: 1, CreatedAt);

        Assert.True(result.IsSuccess);
        var ipAddress = result.Value;
        Assert.Equal("44.255.255.254", ipAddress.Ip);
        Assert.Equal(1, ipAddress.CountryId);
        Assert.Equal(CreatedAt, ipAddress.CreatedAt);
        Assert.Equal(CreatedAt, ipAddress.UpdatedAt);
        Assert.Equal(0, ipAddress.Id);
        Assert.Empty(ipAddress.DomainEvents);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("8.8.8.8")]
    [InlineData("10.20.30.40")] // private range: accepted
    [InlineData("127.0.0.1")]   // loopback: accepted
    public void Create_WhenValidIpV4_ReturnsSuccess(string ip)
    {
        var result = IpAddress.Create(ip, countryId: 1, CreatedAt);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1")]            // IPAddress.TryParse would accept this as 0.0.0.1
    [InlineData("1.2.3")]
    [InlineData("1.2.3.4.5")]
    [InlineData("256.1.1.1")]
    [InlineData("1.1.1.-1")]
    [InlineData("01.1.1.1")]     // leading zero
    [InlineData(" 1.1.1.1")]
    [InlineData("1.1.1.1 ")]
    [InlineData("1..1.1")]
    [InlineData("1.1.1.")]
    [InlineData("+1.1.1.1")]
    [InlineData("1111.1.1.1")]
    [InlineData("::1")]          // IPv6 is not supported
    [InlineData("2001:db8::1")]
    public void Create_WhenNotStrictIpV4_ReturnsInvalidIp(string? ip)
    {
        var result = IpAddress.Create(ip, countryId: 1, CreatedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(IpAddressErrors.InvalidIp, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenCountryIdIsNotPositive_ThrowsArgumentOutOfRangeException(int countryId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IpAddress.Create("8.8.8.8", countryId, CreatedAt));
    }

    [Fact]
    public void ChangeCountry_WhenCountryDiffers_UpdatesCountryAndUpdatedAtAndRaisesEvent()
    {
        var ipAddress = IpAddress.Create("8.8.8.8", countryId: 1, CreatedAt).Value;

        var changed = ipAddress.ChangeCountry(countryId: 2, Later);

        Assert.True(changed);
        Assert.Equal(2, ipAddress.CountryId);
        Assert.Equal(Later, ipAddress.UpdatedAt);
        Assert.Equal(CreatedAt, ipAddress.CreatedAt);
        var domainEvent = Assert.Single(ipAddress.DomainEvents);
        Assert.Equal(new IpAddressCountryChanged("8.8.8.8"), domainEvent);
    }

    [Fact]
    public void ChangeCountry_WhenCountryIsTheSame_ChangesNothing()
    {
        var ipAddress = IpAddress.Create("8.8.8.8", countryId: 1, CreatedAt).Value;

        var changed = ipAddress.ChangeCountry(countryId: 1, Later);

        Assert.False(changed);
        Assert.Equal(CreatedAt, ipAddress.UpdatedAt);
        Assert.Empty(ipAddress.DomainEvents);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeCountry_WhenCountryIdIsNotPositive_ThrowsArgumentOutOfRangeException(int countryId)
    {
        var ipAddress = IpAddress.Create("8.8.8.8", countryId: 1, CreatedAt).Value;

        Assert.Throws<ArgumentOutOfRangeException>(() => ipAddress.ChangeCountry(countryId, Later));
    }
}
