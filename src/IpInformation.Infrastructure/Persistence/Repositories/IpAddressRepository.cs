using IpInformation.Domain.IpAddresses;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence.Repositories;

internal sealed class IpAddressRepository(ApplicationDbContext context) : IIpAddressRepository
{
    public Task<IpAddress?> GetByIpAsync(string ip, CancellationToken cancellationToken) =>
        context.IpAddresses.FirstOrDefaultAsync(i => i.Ip == ip, cancellationToken);

    public async Task<IReadOnlyList<IpAddress>> GetBatchAsync(int afterId, int size, CancellationToken cancellationToken) =>
        await context.IpAddresses
            .Where(i => i.Id > afterId)
            .OrderBy(i => i.Id)
            .Take(size)
            .ToListAsync(cancellationToken);

    public void Add(IpAddress ipAddress) => context.IpAddresses.Add(ipAddress);

    public void Remove(IpAddress ipAddress) => context.IpAddresses.Remove(ipAddress);
}
