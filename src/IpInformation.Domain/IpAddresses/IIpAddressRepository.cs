namespace IpInformation.Domain.IpAddresses;

public interface IIpAddressRepository
{
    Task<IpAddress?> GetByIpAsync(string ip, CancellationToken cancellationToken);

    /// <summary>
    /// Returns up to <paramref name="size"/> IP addresses with <c>Id</c> greater than <paramref name="afterId"/>,
    /// ordered by <c>Id</c> (keyset pagination: stable even when rows are added or removed between batches).
    /// </summary>
    Task<IReadOnlyList<IpAddress>> GetBatchAsync(int afterId, int size, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new IP address. It is stored on <see cref="Abstractions.IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    void Add(IpAddress ipAddress);

    /// <summary>
    /// Deletes an IP address on <see cref="Abstractions.IUnitOfWork.SaveChangesAsync"/>.
    /// Call <see cref="IpAddress.Remove"/> first, so the removal is announced.
    /// </summary>
    void Remove(IpAddress ipAddress);
}
