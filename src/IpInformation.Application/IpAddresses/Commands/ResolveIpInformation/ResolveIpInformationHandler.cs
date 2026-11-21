using IpInformation.Application.Abstractions.Caching;
using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Application.Countries;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.Countries;
using IpInformation.Domain.IpAddresses;
using MediatR;

namespace IpInformation.Application.IpAddresses.Commands.ResolveIpInformation;

public sealed class ResolveIpInformationHandler(
    IIpInformationCache cache,
    IIpAddressRepository ipAddresses,
    ICountryRepository countries,
    CountryResolver countryResolver,
    IIp2cClient ip2c,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ResolveIpInformationCommand, Result<IpInformationDto>>
{
    public async Task<Result<IpInformationDto>> Handle(ResolveIpInformationCommand command, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync(command.Ip, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        var information = await FindInDatabaseAsync(command.Ip, cancellationToken);
        if (information is null)
        {
            var fetched = await FetchFromIp2cAndSaveAsync(command.Ip, cancellationToken);
            if (fetched.IsFailure)
            {
                return fetched;
            }

            information = fetched.Value;
        }

        await cache.SetAsync(command.Ip, information, cancellationToken);
        return Result.Success(information);
    }

    private async Task<IpInformationDto?> FindInDatabaseAsync(string ip, CancellationToken cancellationToken)
    {
        var ipAddress = await ipAddresses.GetByIpAsync(ip, cancellationToken);
        if (ipAddress is null)
        {
            return null;
        }

        var country = await countries.GetByIdAsync(ipAddress.CountryId, cancellationToken)
            ?? throw new InvalidOperationException($"Country {ipAddress.CountryId} of IP {ip} does not exist.");

        return ToDto(country);
    }

    private async Task<Result<IpInformationDto>> FetchFromIp2cAndSaveAsync(string ip, CancellationToken cancellationToken)
    {
        var ip2cCountry = await ip2c.LookupAsync(ip, cancellationToken);
        if (ip2cCountry is null)
        {
            return Result.Failure<IpInformationDto>(IpInformationErrors.NotFound);
        }

        var resolved = await countryResolver.GetOrCreateAsync(ip2cCountry, cancellationToken);
        if (resolved.IsFailure)
        {
            // Bad data from an external service is not the caller's fault: unexpected failure.
            throw new InvalidOperationException($"IP2C returned an invalid country for IP {ip}: {resolved.Error.Description}");
        }

        var country = resolved.Value;

        var ipAddress = IpAddress.Create(ip, country.Id, timeProvider.GetUtcNow().UtcDateTime);
        if (ipAddress.IsFailure)
        {
            return Result.Failure<IpInformationDto>(ipAddress.Error);
        }

        ipAddresses.Add(ipAddress.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToDto(country));
    }

    private static IpInformationDto ToDto(Country country) =>
        new(country.Name, country.TwoLetterCode, country.ThreeLetterCode);
}
