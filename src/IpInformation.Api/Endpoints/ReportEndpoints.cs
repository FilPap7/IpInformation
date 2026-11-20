using IpInformation.Api.Extensions;
using IpInformation.Application.Countries.Queries.GetCountryReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IpInformation.Api.Endpoints;

internal static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/countries", async (
                [FromQuery] string[]? twoLetterCodes,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetCountryReportQuery(twoLetterCodes), cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("GetCountryReport")
            .WithTags("Reports")
            .WithSummary("Returns how many IPs each country has and when an IP of it was last updated.")
            .WithDescription("Repeat twoLetterCodes to filter, e.g. ?twoLetterCodes=GR&twoLetterCodes=US. Omit it for all countries.")
            .Produces<IReadOnlyList<CountryReportItemDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
