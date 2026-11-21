using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Application.Countries;
using IpInformation.Application.IpAddresses;
using IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;
using IpInformation.Application.Tests.Fakes;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.Tests.IpAddresses.Commands.ResolveIpInformation;

public class ResolveIpInformationHandlerTests
{
    private const string Ip = "8.8.8.8";
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly IpInformationDto UnitedStates = new("United States", "US", "USA");

    private readonly FakeIpInformationCache _cache = new();
    private readonly FakeDatabase _database = new();
    private readonly FakeIp2cClient _ip2c = new();
    private readonly ResolveIpInformationHandler _handler;

    public ResolveIpInformationHandlerTests()
    {
        var timeProvider = new FixedTimeProvider(Now);
        _handler = new ResolveIpInformationHandler(
            _cache, _database, _database, new CountryResolver(_database, _database, timeProvider), _ip2c, _database, timeProvider);
    }

    private Task<Result<IpInformationDto>> Resolve(string ip = Ip) =>
        _handler.Handle(new ResolveIpInformationCommand(ip), CancellationToken.None);

    private async Task<Country> SeedCountryAsync(string name, string twoLetterCode, string threeLetterCode)
    {
        var country = Country.Create(name, twoLetterCode, threeLetterCode, Now).Value;
        _database.Add(country);
        await _database.SaveChangesAsync(CancellationToken.None);
        return country;
    }

    [Fact]
    public async Task Handle_WhenInCache_ReturnsItWithoutDatabaseOrIp2c()
    {
        _cache.Entries[Ip] = UnitedStates;

        var result = await Resolve();

        Assert.True(result.IsSuccess);
        Assert.Equal(UnitedStates, result.Value);
        Assert.Equal(0, _ip2c.Calls);
        Assert.Empty(_database.IpAddresses);
    }

    [Fact]
    public async Task Handle_WhenInDatabase_ReturnsItAndCachesItWithoutIp2c()
    {
        var country = await SeedCountryAsync("United States", "US", "USA");
        _database.Add(IpAddress.Create(Ip, country.Id, Now).Value);
        await _database.SaveChangesAsync(CancellationToken.None);

        var result = await Resolve();

        Assert.True(result.IsSuccess);
        Assert.Equal(UnitedStates, result.Value);
        Assert.Equal(UnitedStates, _cache.Entries[Ip]);
        Assert.Equal(0, _ip2c.Calls);
    }

    [Fact]
    public async Task Handle_WhenNewIpAndNewCountry_SavesBothAndCachesIt()
    {
        _ip2c.Responses[Ip] = new Ip2cCountry("United States", "US", "USA");

        var result = await Resolve();

        Assert.True(result.IsSuccess);
        Assert.Equal(UnitedStates, result.Value);
        var country = Assert.Single(_database.Countries);
        Assert.Equal("US", country.TwoLetterCode);
        Assert.Equal(Now, country.CreatedAt);
        var ipAddress = Assert.Single(_database.IpAddresses);
        Assert.Equal(Ip, ipAddress.Ip);
        Assert.Equal(country.Id, ipAddress.CountryId);
        Assert.Equal(Now, ipAddress.CreatedAt);
        Assert.Equal(UnitedStates, _cache.Entries[Ip]);
    }

    [Fact]
    public async Task Handle_WhenNewIpAndExistingCountry_ReusesTheCountry()
    {
        var country = await SeedCountryAsync("United States", "US", "USA");
        _ip2c.Responses[Ip] = new Ip2cCountry("United States", "US", "USA");

        var result = await Resolve();

        Assert.True(result.IsSuccess);
        Assert.Single(_database.Countries);
        Assert.Equal(country.Id, Assert.Single(_database.IpAddresses).CountryId);
    }

    [Fact]
    public async Task Handle_WhenIp2cDoesNotKnowTheIp_ReturnsNotFoundAndStoresNothing()
    {
        var result = await Resolve();

        Assert.True(result.IsFailure);
        Assert.Equal(IpInformationErrors.NotFound, result.Error);
        Assert.Empty(_database.Countries);
        Assert.Empty(_database.IpAddresses);
        Assert.Empty(_cache.Entries);
    }

    [Fact]
    public async Task Handle_WhenIp2cReturnsAnInvalidCountry_ThrowsAndStoresNothing()
    {
        _ip2c.Responses[Ip] = new Ip2cCountry("United States", "U1", "USA");

        await Assert.ThrowsAsync<InvalidOperationException>(() => Resolve());

        Assert.Empty(_database.Countries);
        Assert.Empty(_database.IpAddresses);
        Assert.Empty(_cache.Entries);
    }
}
