using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.IntegrationTests.Database;

public class SchemaConstraintTests(SqlServerFixture database) : DatabaseTest(database)
{
    [Fact]
    public async Task SaveChanges_WhenTheIpAlreadyExists_IsRejectedByTheUniqueIndex()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        await AddIpAsync("8.8.8.8", greece);

        await using var context = Database.CreateContext();
        context.IpAddresses.Add(IpAddress.Create("8.8.8.8", greece.Id, CreatedAt).Value);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenTheTwoLetterCodeAlreadyExists_IsRejectedByTheUniqueIndex()
    {
        await AddCountryAsync("Greece", "GR", "GRC");

        await using var context = Database.CreateContext();
        context.Countries.Add(Country.Create("Hellas", "GR", "GRC", CreatedAt).Value);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenDeletingACountryThatHasIps_IsRejectedByTheForeignKey()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        await AddIpAsync("8.8.8.8", greece);

        await using var context = Database.CreateContext();
        context.Countries.Remove(await context.Countries.SingleAsync());

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Read_Always_ReturnsDatesAsUtc()
    {
        var greece = await AddCountryAsync("Greece", "GR", "GRC");
        await AddIpAsync("8.8.8.8", greece);

        await using var context = Database.CreateContext();
        var country = await context.Countries.SingleAsync();
        var ipAddress = await context.IpAddresses.SingleAsync();

        Assert.Equal(DateTimeKind.Utc, country.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, ipAddress.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, ipAddress.UpdatedAt.Kind);
        Assert.Equal(CreatedAt, ipAddress.UpdatedAt);
    }
}
