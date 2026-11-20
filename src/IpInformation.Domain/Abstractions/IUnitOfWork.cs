namespace IpInformation.Domain.Abstractions;

/// <summary>
/// Saves every change tracked by the repositories in one call.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
