using IpInformation.Api.Extensions;
using IpInformation.Application.IpAddresses;
using IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;
using MediatR;

namespace IpInformation.Api.Endpoints;

internal static class IpInformationEndpoints
{
    public static IEndpointRouteBuilder MapIpInformationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ip-information/{ip}", async (string ip, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new ResolveIpInformationCommand(ip), cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .WithName("GetIpInformation")
            .WithTags("IP information")
            .WithSummary("Returns the country of an IPv4 address.")
            .WithDescription("Looks in the cache, then the database, then IP2C. A new IP found by IP2C is stored for later use.")
            .Produces<IpInformationDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
