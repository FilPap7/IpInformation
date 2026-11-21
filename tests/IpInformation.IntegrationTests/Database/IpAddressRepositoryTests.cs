using IpInformation.Infrastructure.Persistence.Repositories;

namespace IpInformation.IntegrationTests.Database;

public class IpAddressRepositoryTests(SqlServerFixture database) : DatabaseTest(database)
{
    [Fact]
    public async Task GetBatchAsync_Always_ReturnsTheNextIpsAfterTheGivenIdOrderedById()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        var first = await AddIpAsync("1.1.1.1", greece);
        var second = await AddIpAsync("2.2.2.2", greece);
        var third = await AddIpAsync("3.3.3.3", greece);
        await AddIpAsync("4.4.4.4", greece);

        await using var context = Database.CreateContext();
        var batch = await new IpAddressRepository(context).GetBatchAsync(first.Id, 2, CancellationToken.None);

        Assert.Equal([second.Id, third.Id], batch.Select(i => i.Id));
    }

    [Fact]
    public async Task GetBatchAsync_WhenNoIpsAreLeft_ReturnsAnEmptyBatch()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        var only = await AddIpAsync("1.1.1.1", greece);

        await using var context = Database.CreateContext();
        var batch = await new IpAddressRepository(context).GetBatchAsync(only.Id, 100, CancellationToken.None);

        Assert.Empty(batch);
    }
}
