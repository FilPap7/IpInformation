using IpInformation.Application.Countries.Queries.GetCountryReport;

namespace IpInformation.Application.Abstractions.Data;

/// <summary>
/// Reads the per-country report straight from the database (read-only, projected to the DTO).
/// </summary>
public interface ICountryReportReader
{
    /// <param name="twoLetterCodes">Upper-case codes to include, or <c>null</c> for all countries.</param>
    /// <returns>
    /// One item per country that has at least one IP, ordered by country name.
    /// Codes that match no country are ignored.
    /// </returns>
    Task<IReadOnlyList<CountryReportItemDto>> GetAsync(
        IReadOnlyCollection<string>? twoLetterCodes,
        CancellationToken cancellationToken);
}
