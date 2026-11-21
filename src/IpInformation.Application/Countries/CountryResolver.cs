using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;

namespace IpInformation.Application.Countries;

/// <summary>
/// Finds the stored country of an IP2C answer by its two-letter code, or creates and saves it.
/// Shared by the IP lookup and the hourly refresh, so both create countries the same way.
/// </summary>
public sealed class CountryResolver(ICountryRepository countries, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    /// <returns>
    /// The saved country (it has an <c>Id</c>), or a failure when IP2C returned a country the domain rejects.
    /// </returns>
    public async Task<Result<Country>> GetOrCreateAsync(Ip2cCountry ip2cCountry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ip2cCountry);

        var country = await countries.GetByTwoLetterCodeAsync(ip2cCountry.TwoLetterCode, cancellationToken);
        if (country is not null)
        {
            return Result.Success(country);
        }

        var created = Country.Create(
            ip2cCountry.Name,
            ip2cCountry.TwoLetterCode,
            ip2cCountry.ThreeLetterCode,
            timeProvider.GetUtcNow().UtcDateTime);

        if (created.IsFailure)
        {
            return created;
        }

        countries.Add(created.Value);

        // Saved now because callers need the Id of the country to reference it from an IP.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return created;
    }
}
