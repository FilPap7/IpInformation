namespace IpInformation.Domain.IpAddresses;

public interface IIpAddressRepository
{
    Task<IpAddress?> GetByIpAsync(string ip, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new IP address. It is stored on <see cref="Abstractions.IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    void Add(IpAddress ipAddress);
}
