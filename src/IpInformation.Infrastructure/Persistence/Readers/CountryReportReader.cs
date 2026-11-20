using IpInformation.Application.Abstractions.Data;
using IpInformation.Application.Countries.Queries.GetCountryReport;
using Microsoft.EntityFrameworkCore;

namespace IpInformation.Infrastructure.Persistence.Readers;

/// <summary>
/// The country report as a single EF Core LINQ query, translated to one SQL GROUP BY. No raw SQL.
/// </summary>
internal sealed class CountryReportReader(ApplicationDbContext context) : ICountryReportReader
{
    public async Task<IReadOnlyList<CountryReportItemDto>> GetAsync(
        IReadOnlyCollection<string>? twoLetterCodes,
        CancellationToken cancellationToken)
    {
        var countries = context.Countries.AsNoTracking();

        if (twoLetterCodes is not null)
        {
            countries = countries.Where(c => twoLetterCodes.Contains(c.TwoLetterCode));
        }

        // Inner join: countries without IPs are left out.
        // Grouped by Id as well as Name, so two countries never merge even if their names match.
        var report =
            from country in countries
            join ipAddress in context.IpAddresses.AsNoTracking() on country.Id equals ipAddress.CountryId
            group ipAddress by new { country.Id, country.Name } into byCountry
            orderby byCountry.Key.Name
            select new CountryReportItemDto(
                byCountry.Key.Name,
                byCountry.Count(),
                byCountry.Max(i => i.UpdatedAt));

        return await report.ToListAsync(cancellationToken);
    }
}
