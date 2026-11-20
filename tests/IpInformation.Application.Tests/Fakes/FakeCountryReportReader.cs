using IpInformation.Application.Abstractions.Data;
using IpInformation.Application.Countries.Queries.GetCountryReport;

namespace IpInformation.Application.Tests.Fakes;

/// <summary>
/// Returns <see cref="Report"/> and records the filter it was called with.
/// </summary>
internal sealed class FakeCountryReportReader : ICountryReportReader
{
    public IReadOnlyList<CountryReportItemDto> Report { get; set; } = [];

    public IReadOnlyCollection<string>? ReceivedTwoLetterCodes { get; private set; }

    public Task<IReadOnlyList<CountryReportItemDto>> GetAsync(
        IReadOnlyCollection<string>? twoLetterCodes,
        CancellationToken cancellationToken)
    {
        ReceivedTwoLetterCodes = twoLetterCodes;
        return Task.FromResult(Report);
    }
}
