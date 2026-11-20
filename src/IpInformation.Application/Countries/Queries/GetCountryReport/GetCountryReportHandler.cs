using IpInformation.Application.Abstractions.Data;
using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.Countries.Queries.GetCountryReport;

public sealed class GetCountryReportHandler(ICountryReportReader reader)
    : IRequestHandler<GetCountryReportQuery, Result<IReadOnlyList<CountryReportItemDto>>>
{
    public async Task<Result<IReadOnlyList<CountryReportItemDto>>> Handle(
        GetCountryReportQuery query,
        CancellationToken cancellationToken)
    {
        // Codes are stored upper-case. An empty filter means "all countries", like null.
        IReadOnlyCollection<string>? twoLetterCodes = query.TwoLetterCodes is { Count: > 0 }
            ? query.TwoLetterCodes.Select(c => c.ToUpperInvariant()).Distinct().ToArray()
            : null;

        var report = await reader.GetAsync(twoLetterCodes, cancellationToken);

        return Result.Success(report);
    }
}
