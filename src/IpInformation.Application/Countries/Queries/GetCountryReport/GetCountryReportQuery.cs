using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.Countries.Queries.GetCountryReport;

/// <summary>
/// How many IPs each country has and when an IP of it was last updated.
/// </summary>
/// <param name="TwoLetterCodes">Countries to include; <c>null</c> or empty for all.</param>
public sealed record GetCountryReportQuery(IReadOnlyCollection<string>? TwoLetterCodes)
    : IRequest<Result<IReadOnlyList<CountryReportItemDto>>>;
