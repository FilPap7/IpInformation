using IpInformation.Application;
using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Application.Countries;
using IpInformation.Application.IpAddresses;
using IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using IpInformation.Infrastructure.Caching;
using IpInformation.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.IntegrationTests.Database;

/// <summary>
/// The refresh of one batch end to end: real database, real domain event dispatch and real cache;
/// only IP2C is replaced.
/// </summary>
public class RefreshIpAddressBatchTests(SqlServerFixture database) : DatabaseTest(database)
{
    private static readonly IpInformationDto CachedUnitedStates = new("United States", "US", "USA");

    private sealed class StubIp2cClient(Dictionary<string, Ip2cCountry> responses) : IIp2cClient
    {
        public Task<Ip2cCountry?> LookupAsync(string ip, CancellationToken cancellationToken) =>
            Task.FromResult(responses.GetValueOrDefault(ip));
    }

    private readonly IpInformationCache _cache = new();

    private async Task RefreshAsync(Dictionary<string, Ip2cCountry> ip2cResponses)
    {
        // The handlers of domain events (cache invalidation) come from the real Application registration.
        await using var services = new ServiceCollection()
            .AddApplication()
            .AddSingleton<IIpInformationCache>(_cache)
            .BuildServiceProvider();

        await using var context = Database.CreateContext(services);
        var timeProvider = TimeProvider.System;
        var handler = new RefreshIpAddressBatchHandler(
            new IpAddressRepository(context),
            new CountryResolver(new CountryRepository(context), context, timeProvider),
            new StubIp2cClient(ip2cResponses),
            context,
            timeProvider);

        var result = await handler.Handle(new RefreshIpAddressBatchCommand(0, 100), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    private async Task<Dictionary<string, string>> CountryCodeByIpAsync()
    {
        await using var context = Database.CreateContext();
        return await (
            from ipAddress in context.IpAddresses
            join country in context.Countries on ipAddress.CountryId equals country.Id
            select new { ipAddress.Ip, country.TwoLetterCode })
            .ToDictionaryAsync(x => x.Ip, x => x.TwoLetterCode);
    }

    private async Task<Country> SeedUnitedStatesWithCachedIpsAsync(params string[] ips)
    {
        var unitedStates = await AddCountryAsync("United States", "US", "USA");
        foreach (var ip in ips)
        {
            await AddIpAsync(ip, unitedStates);
            await _cache.SetAsync(ip, CachedUnitedStates, CancellationToken.None);
        }

        return unitedStates;
    }

    [Fact]
    public async Task Refresh_WhenTheCountryChanged_SavesTheNewCountryAndInvalidatesOnlyThatIp()
    {
        await SeedUnitedStatesWithCachedIpsAsync("8.8.8.8", "4.2.2.2");

        await RefreshAsync(new()
        {
            ["8.8.8.8"] = new Ip2cCountry("Greece", "GR", "GRC"),
            ["4.2.2.2"] = new Ip2cCountry("United States", "US", "USA"),
        });

        var codes = await CountryCodeByIpAsync();
        Assert.Equal("GR", codes["8.8.8.8"]);
        Assert.Equal("US", codes["4.2.2.2"]);
        Assert.Null(await _cache.GetAsync("8.8.8.8", CancellationToken.None));
        Assert.NotNull(await _cache.GetAsync("4.2.2.2", CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_WhenTheCountryChanged_UpdatesUpdatedAt()
    {
        await SeedUnitedStatesWithCachedIpsAsync("8.8.8.8");

        await RefreshAsync(new() { ["8.8.8.8"] = new Ip2cCountry("Greece", "GR", "GRC") });

        await using var context = Database.CreateContext();
        var ipAddress = await context.IpAddresses.SingleAsync();
        Assert.True(ipAddress.UpdatedAt > CreatedAt);
    }

    [Fact]
    public async Task Refresh_WhenIp2cNoLongerKnowsTheIp_DeletesItAndInvalidatesTheCache()
    {
        await SeedUnitedStatesWithCachedIpsAsync("8.8.8.8", "4.2.2.2");

        await RefreshAsync(new() { ["4.2.2.2"] = new Ip2cCountry("United States", "US", "USA") });

        var codes = await CountryCodeByIpAsync();
        Assert.Equal(["4.2.2.2"], codes.Keys);
        Assert.Null(await _cache.GetAsync("8.8.8.8", CancellationToken.None));

        await using var context = Database.CreateContext();
        Assert.Equal(1, await context.Countries.CountAsync()); // The country stays.
    }

    [Fact]
    public async Task SaveChanges_WhenTheSaveFails_DoesNotInvalidateTheCache()
    {
        var unitedStates = await SeedUnitedStatesWithCachedIpsAsync("8.8.8.8");
        var greece = await AddCountryAsync("Greece", "GR", "GRC");

        await using var services = new ServiceCollection()
            .AddApplication()
            .AddSingleton<IIpInformationCache>(_cache)
            .BuildServiceProvider();
        await using var context = Database.CreateContext(services);

        var ipAddress = await context.IpAddresses.SingleAsync();
        ipAddress.ChangeCountry(greece.Id, DateTime.UtcNow);
        context.IpAddresses.Add(IpAddress.Create("8.8.8.8", unitedStates.Id, CreatedAt).Value); // Duplicate IP.

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.NotNull(await _cache.GetAsync("8.8.8.8", CancellationToken.None));
    }
}
