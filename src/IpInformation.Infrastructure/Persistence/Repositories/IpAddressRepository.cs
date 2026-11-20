using IpInformation.Domain.IpAddresses;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence.Repositories;

internal sealed class IpAddressRepository(ApplicationDbContext context) : IIpAddressRepository
{
    public Task<IpAddress?> GetByIpAsync(string ip, CancellationToken cancellationToken) =>
        context.IpAddresses.FirstOrDefaultAsync(i => i.Ip == ip, cancellationToken);

    public void Add(IpAddress ipAddress) => context.IpAddresses.Add(ipAddress);
}
