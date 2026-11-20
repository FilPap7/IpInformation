using IpInformation.Domain.Countries;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence.Repositories;

internal sealed class CountryRepository(ApplicationDbContext context) : ICountryRepository
{
    public Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        context.Countries.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Country?> GetByTwoLetterCodeAsync(string twoLetterCode, CancellationToken cancellationToken) =>
        context.Countries.FirstOrDefaultAsync(c => c.TwoLetterCode == twoLetterCode, cancellationToken);

    public void Add(Country country) => context.Countries.Add(country);
}
