using IpInformation.Infrastructure.Events;
using IpInformation.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IpInformation.IntegrationTests.Database;

/// <summary>
/// A real SQL Server database for tests, shared by every test in <see cref="DatabaseCollection"/>.
/// Migrated once per test run; emptied before each test by <see cref="DatabaseTest"/>.
/// </summary>
/// <remarks>
/// Defaults to <c>IpInformation_Tests</c> on the local SQL Server Express. Set the
/// <see cref="ConnectionStringVariable"/> environment variable to use another server (e.g. a container).
/// </remarks>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string ConnectionStringVariable = "IPINFORMATION_TEST_DATABASE";

    private const string DefaultConnectionString =
        @"Server=.\SQLEXPRESS;Database=IpInformation_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    // Every test deletes all rows, so never run against a database that is not meant for tests.
    private const string RequiredDatabaseSuffix = "_Tests";

    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SqlServerFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) is { Length: > 0 } configured
            ? configured
            : DefaultConnectionString;

        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        if (!database.EndsWith(RequiredDatabaseSuffix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The test database must end with '{RequiredDatabaseSuffix}' because tests delete all its data; got '{database}'.");
        }

        _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connectionString).Options;
    }

    /// <summary>
    /// A new context. Domain events are dispatched to the handlers registered in <paramref name="services"/>
    /// (none by default).
    /// </summary>
    public ApplicationDbContext CreateContext(IServiceProvider? services = null) =>
        new(_options, new DomainEventDispatcher(services ?? new ServiceCollection().BuildServiceProvider()));

    public async Task InitializeAsync()
    {
        // Creates the database on the first run and applies any migration it is missing.
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Deletes every row, IPs first because of the foreign key. EF Core bulk delete, no raw SQL.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var context = CreateContext();
        await context.IpAddresses.ExecuteDeleteAsync();
        await context.Countries.ExecuteDeleteAsync();
    }
}

/// <summary>
/// Tests that share the database run one at a time, so they never see each other's rows.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "Database";
}
