using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using IpInformation.Infrastructure.Events;
using IpInformation.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<IpAddress> IpAddresses => Set<IpAddress>();

    /// <summary>
    /// Saves, then dispatches the domain events raised by the saved aggregates.
    /// If the save fails, no event is dispatched. If a handler fails, the changes are already saved.
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        var rows = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        // Cleared before dispatching, so a handler that saves again does not dispatch these events twice.
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        if (domainEvents.Count > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return rows;
    }

    /// <summary>
    /// Not supported: the synchronous save cannot dispatch domain events, so it would lose them silently.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new NotSupportedException("Use SaveChangesAsync, which dispatches domain events after saving.");

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
