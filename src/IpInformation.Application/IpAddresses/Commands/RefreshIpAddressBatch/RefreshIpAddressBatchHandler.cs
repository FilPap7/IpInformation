using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Application.Countries;
using IpInformation.Domain.Abstractions;
using IpInformation.Domain.IpAddresses;
using MediatR;

namespace IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;

/// <summary>
/// For each IP of the batch, asks IP2C for its current country and:
/// moves it when the country changed, deletes it when IP2C no longer knows it,
/// and skips it when IP2C fails (it is retried next cycle). Saves once per batch.
/// </summary>
/// <remarks>
/// The cache is not touched here: moving or removing an IP raises a domain event, and its handler
/// invalidates the cached information after the save.
/// </remarks>
public sealed class RefreshIpAddressBatchHandler(
    IIpAddressRepository ipAddresses,
    CountryResolver countryResolver,
    IIp2cClient ip2c,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshIpAddressBatchCommand, Result<RefreshIpAddressBatchResult>>
{
    public async Task<Result<RefreshIpAddressBatchResult>> Handle(
        RefreshIpAddressBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await ipAddresses.GetBatchAsync(command.AfterId, command.BatchSize, cancellationToken);
        if (batch.Count == 0)
        {
            return Result.Success(new RefreshIpAddressBatchResult(0, command.AfterId, 0, 0, []));
        }

        var changed = 0;
        var removed = 0;
        var failedIps = new List<string>();

        foreach (var ipAddress in batch)
        {
            switch (await RefreshAsync(ipAddress, cancellationToken))
            {
                case Outcome.Changed: changed++; break;
                case Outcome.Removed: removed++; break;
                case Outcome.Failed: failedIps.Add(ipAddress.Ip); break;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshIpAddressBatchResult(batch.Count, batch[^1].Id, changed, removed, failedIps));
    }

    private async Task<Outcome> RefreshAsync(IpAddress ipAddress, CancellationToken cancellationToken)
    {
        Ip2cCountry? ip2cCountry;
        try
        {
            ip2cCountry = await ip2c.LookupAsync(ipAddress.Ip, cancellationToken);
        }
        catch (Exception exception) when (IsIp2cFailure(exception, cancellationToken))
        {
            return Outcome.Failed;
        }

        if (ip2cCountry is null)
        {
            // Same state as an IP that was never stored: a lookup of it returns "not found".
            ipAddress.Remove();
            ipAddresses.Remove(ipAddress);
            return Outcome.Removed;
        }

        var country = await countryResolver.GetOrCreateAsync(ip2cCountry, cancellationToken);
        if (country.IsFailure)
        {
            return Outcome.Failed;
        }

        return ipAddress.ChangeCountry(country.Value.Id, timeProvider.GetUtcNow().UtcDateTime)
            ? Outcome.Changed
            : Outcome.Unchanged;
    }

    // IP2C unreachable, error response or timeout. A cancellation of the job itself is not a failure of one IP.
    private static bool IsIp2cFailure(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    private enum Outcome
    {
        Unchanged,
        Changed,
        Removed,
        Failed,
    }
}
