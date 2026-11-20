using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using IpInformation.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<IpAddress> IpAddresses => Set<IpAddress>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All dates are UTC: applied once here instead of on every DateTime property.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    // DbContext.SaveChangesAsync returns the number of rows; the domain does not need it.
    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);
}
