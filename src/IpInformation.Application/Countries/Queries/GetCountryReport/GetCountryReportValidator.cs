using FluentValidation;
using IpInformation.Domain.Countries;

namespace IpInformation.Application.Countries.Queries.GetCountryReport;

/// <summary>
/// Every given code must be a valid two-letter code. Reuses the domain rule.
/// </summary>
public sealed class GetCountryReportValidator : AbstractValidator<GetCountryReportQuery>
{
    public GetCountryReportValidator()
    {
        RuleForEach(q => q.TwoLetterCodes)
            .Must(Country.IsValidTwoLetterCode)
            .WithErrorCode(CountryErrors.InvalidTwoLetterCode.Code)
            .WithMessage(CountryErrors.InvalidTwoLetterCode.Description)
            .When(q => q.TwoLetterCodes is not null);
    }
}
