using IpInformation.Application.Countries.Queries.GetCountryReport;
using IpInformation.Infrastructure.Persistence.Readers;

namespace IpInformation.IntegrationTests.Database;

public class CountryReportReaderTests(SqlServerFixture database) : DatabaseTest(database)
{
    private async Task<IReadOnlyList<CountryReportItemDto>> GetReportAsync(IReadOnlyCollection<string>? codes)
    {
        await using var context = Database.CreateContext();
        return await new CountryReportReader(context).GetAsync(codes, CancellationToken.None);
    }

    private async Task SeedAsync()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        var unitedStates = await AddCountryAsync("United States", "US", "USA");
        await AddCountryAsync("France", "FR", "FRA"); // No IPs.

        await AddIpAsync("195.251.255.1", greece, CreatedAt);
        await AddIpAsync("62.103.0.1", greece, CreatedAt.AddHours(2));
        await AddIpAsync("8.8.8.8", unitedStates, CreatedAt.AddHours(1));
    }

    [Fact]
    public async Task GetAsync_WhenNotFiltered_CountsIpsAndLatestUpdatePerCountryOrderedByName()
    {
        await SeedAsync();

        var report = await GetReportAsync(null);

        Assert.Equal(
            [
                new CountryReportItemDto("Greece", 2, CreatedAt.AddHours(2)),
                new CountryReportItemDto("United States", 1, CreatedAt.AddHours(1)),
            ],
            report);
    }

    [Fact]
    public async Task GetAsync_WhenFilteredByCodes_ReturnsOnlyThoseCountriesAndIgnoresUnknownCodes()
    {
        await SeedAsync();

        var report = await GetReportAsync(["US", "ZZ"]);

        var item = Assert.Single(report);
        Assert.Equal("United States", item.CountryName);
    }

    [Fact]
    public async Task GetAsync_Always_ReturnsLastAddressUpdatedAsUtc()
    {
        await SeedAsync();

        var report = await GetReportAsync(null);

        Assert.All(report, item => Assert.Equal(DateTimeKind.Utc, item.LastAddressUpdated.Kind));
    }
}
