namespace IpInformation.Domain.Countries;

public interface ICountryRepository
{
    Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Country?> GetByTwoLetterCodeAsync(string twoLetterCode, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new country. It is stored, and gets its <c>Id</c>, on <see cref="Abstractions.IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    void Add(Country country);
}
