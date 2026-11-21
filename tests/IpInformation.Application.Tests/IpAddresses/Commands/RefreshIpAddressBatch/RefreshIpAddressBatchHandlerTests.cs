using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Application.Countries;
using IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;
using IpInformation.Application.Tests.Fakes;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.Tests.IpAddresses.Commands.RefreshIpAddressBatch;

public class RefreshIpAddressBatchHandlerTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = CreatedAt.AddHours(1);

    private static readonly Ip2cCountry UnitedStates = new("United States", "US", "USA");
    private static readonly Ip2cCountry Greece = new("Greece", "GR", "GRC");

    private readonly FakeDatabase _database = new();
    private readonly FakeIp2cClient _ip2c = new();
    private readonly RefreshIpAddressBatchHandler _handler;

    public RefreshIpAddressBatchHandlerTests()
    {
        var timeProvider = new FixedTimeProvider(Now);
        _handler = new RefreshIpAddressBatchHandler(
            _database, new CountryResolver(_database, _database, timeProvider), _ip2c, _database, timeProvider);
    }

    private Task<Result<RefreshIpAddressBatchResult>> Refresh(int afterId = 0, int batchSize = 100) =>
        _handler.Handle(new RefreshIpAddressBatchCommand(afterId, batchSize), CancellationToken.None);

    private async Task<Country> SeedCountryAsync(Ip2cCountry country)
    {
        var created = Country.Create(country.Name, country.TwoLetterCode, country.ThreeLetterCode, CreatedAt).Value;
        _database.Add(created);
        await _database.SaveChangesAsync(CancellationToken.None);
        return created;
    }

    private async Task<IpAddress> SeedIpAsync(string ip, Country country)
    {
        var ipAddress = IpAddress.Create(ip, country.Id, CreatedAt).Value;
        _database.Add(ipAddress);
        await _database.SaveChangesAsync(CancellationToken.None);
        return ipAddress;
    }

    [Fact]
    public async Task Handle_WhenTheCountryIsTheSame_LeavesTheIpUnchanged()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var ipAddress = await SeedIpAsync("8.8.8.8", us);
        _ip2c.Responses["8.8.8.8"] = UnitedStates;

        var result = await Refresh();

        Assert.Equal(new { Processed = 1, Changed = 0, Removed = 0 },
            new { result.Value.Processed, result.Value.Changed, result.Value.Removed });
        Assert.Equal(us.Id, ipAddress.CountryId);
        Assert.Equal(CreatedAt, ipAddress.UpdatedAt);
        Assert.Empty(ipAddress.DomainEvents);
    }

    [Fact]
    public async Task Handle_WhenTheCountryChangedToAStoredCountry_MovesTheIpAndRaisesTheEvent()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var gr = await SeedCountryAsync(Greece);
        var ipAddress = await SeedIpAsync("8.8.8.8", us);
        _ip2c.Responses["8.8.8.8"] = Greece;

        var result = await Refresh();

        Assert.Equal(1, result.Value.Changed);
        Assert.Equal(gr.Id, ipAddress.CountryId);
        Assert.Equal(Now, ipAddress.UpdatedAt);
        Assert.Contains(new IpAddressCountryChanged("8.8.8.8"), ipAddress.DomainEvents);
    }

    [Fact]
    public async Task Handle_WhenTheCountryChangedToANewCountry_CreatesTheCountryAndMovesTheIp()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var ipAddress = await SeedIpAsync("8.8.8.8", us);
        _ip2c.Responses["8.8.8.8"] = Greece;

        var result = await Refresh();

        Assert.Equal(1, result.Value.Changed);
        var gr = Assert.Single(_database.Countries, c => c.TwoLetterCode == "GR");
        Assert.Equal(gr.Id, ipAddress.CountryId);
    }

    [Fact]
    public async Task Handle_WhenIp2cNoLongerKnowsTheIp_DeletesItAndRaisesTheEvent()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var ipAddress = await SeedIpAsync("8.8.8.8", us);

        var result = await Refresh();

        Assert.Equal(1, result.Value.Removed);
        Assert.Empty(_database.IpAddresses);
        Assert.Contains(new IpAddressRemoved("8.8.8.8"), ipAddress.DomainEvents);
        Assert.Single(_database.Countries); // The country stays, even without IPs.
    }

    [Fact]
    public async Task Handle_WhenIp2cFailsForAnIp_SkipsItAndRefreshesTheOthers()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var failing = await SeedIpAsync("1.1.1.1", us);
        var other = await SeedIpAsync("8.8.8.8", us);
        _ip2c.Failing.Add("1.1.1.1");
        _ip2c.Responses["8.8.8.8"] = Greece;

        var result = await Refresh();

        Assert.Equal(["1.1.1.1"], result.Value.FailedIps);
        Assert.Equal(1, result.Value.Changed);
        Assert.Equal(us.Id, failing.CountryId);
        Assert.NotEqual(us.Id, other.CountryId);
        Assert.Equal(2, _database.IpAddresses.Count);
    }

    [Fact]
    public async Task Handle_WhenIp2cReturnsAnInvalidCountry_SkipsTheIp()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var ipAddress = await SeedIpAsync("8.8.8.8", us);
        _ip2c.Responses["8.8.8.8"] = new Ip2cCountry("Nowhere", "1", "ABC");

        var result = await Refresh();

        Assert.Equal(["8.8.8.8"], result.Value.FailedIps);
        Assert.Equal(us.Id, ipAddress.CountryId);
    }

    [Fact]
    public async Task Handle_Always_ProcessesOnlyTheBatchAfterTheGivenIdAndSavesOnce()
    {
        var us = await SeedCountryAsync(UnitedStates);
        var first = await SeedIpAsync("1.1.1.1", us);
        await SeedIpAsync("2.2.2.2", us);
        var third = await SeedIpAsync("3.3.3.3", us);
        await SeedIpAsync("4.4.4.4", us);
        foreach (var ip in new[] { "1.1.1.1", "2.2.2.2", "3.3.3.3", "4.4.4.4" })
        {
            _ip2c.Responses[ip] = UnitedStates;
        }

        var savesBefore = _database.SaveChangesCalls;

        var result = await Refresh(afterId: first.Id, batchSize: 2);

        Assert.Equal(2, result.Value.Processed);
        Assert.Equal(third.Id, result.Value.LastId);
        Assert.Equal(2, _ip2c.Calls);
        Assert.Equal(savesBefore + 1, _database.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoMoreIps_ReturnsAnEmptyBatchWithoutSaving()
    {
        var result = await Refresh(afterId: 42);

        Assert.Equal(0, result.Value.Processed);
        Assert.Equal(42, result.Value.LastId);
        Assert.Equal(0, _database.SaveChangesCalls);
        Assert.Equal(0, _ip2c.Calls);
    }
}
