using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.IntegrationTests.Database;

/// <summary>
/// Base class for tests against the real database: starts every test with empty tables
/// and offers helpers to insert data through the aggregates.
/// </summary>
/// <remarks>
/// Run only these tests with <c>dotnet test --filter Category=Database</c>, or skip them with
/// <c>--filter Category!=Database</c> on a machine without SQL Server Express.
/// </remarks>
[Collection(DatabaseCollection.Name)]
[Trait("Category", "Database")]
public abstract class DatabaseTest(SqlServerFixture database) : IAsyncLifetime
{
    protected static readonly DateTime CreatedAt = new(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc);

    protected SqlServerFixture Database { get; } = database;

    public Task InitializeAsync() => Database.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<Country> AddCountryAsync(string name, string twoLetterCode, string threeLetterCode)
    {
        var country = Country.Create(name, twoLetterCode, threeLetterCode, CreatedAt).Value;

        await using var context = Database.CreateContext();
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        return country;
    }

    protected async Task<IpAddress> AddIpAsync(string ip, Country country, DateTime? createdAt = null)
    {
        var ipAddress = IpAddress.Create(ip, country.Id, createdAt ?? CreatedAt).Value;

        await using var context = Database.CreateContext();
        context.IpAddresses.Add(ipAddress);
        await context.SaveChangesAsync();
        return ipAddress;
    }
}
