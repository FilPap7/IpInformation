using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;

namespace IpInformation.Application.Tests.Fakes;

/// <summary>
/// In-memory stand-in for the database: added entities become visible, and get an Id, only after SaveChangesAsync.
/// </summary>
internal sealed class FakeDatabase : ICountryRepository, IIpAddressRepository, IUnitOfWork
{
    private readonly List<Entity> _pending = [];
    private int _nextId = 1;

    public List<Country> Countries { get; } = [];

    public List<IpAddress> IpAddresses { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Countries.SingleOrDefault(c => c.Id == id));

    public Task<Country?> GetByTwoLetterCodeAsync(string twoLetterCode, CancellationToken cancellationToken) =>
        Task.FromResult(Countries.SingleOrDefault(c => c.TwoLetterCode == twoLetterCode));

    public Task<IpAddress?> GetByIpAsync(string ip, CancellationToken cancellationToken) =>
        Task.FromResult(IpAddresses.SingleOrDefault(i => i.Ip == ip));

    public void Add(Country country) => _pending.Add(country);

    public void Add(IpAddress ipAddress) => _pending.Add(ipAddress);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;

        foreach (var entity in _pending)
        {
            // Id has a private setter, like with EF Core the database assigns it.
            typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, _nextId++);

            switch (entity)
            {
                case Country country: Countries.Add(country); break;
                case IpAddress ipAddress: IpAddresses.Add(ipAddress); break;
            }
        }

        _pending.Clear();
        return Task.CompletedTask;
    }
}
