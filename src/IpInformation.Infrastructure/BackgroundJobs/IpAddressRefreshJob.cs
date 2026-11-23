using IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IpInformation.Infrastructure.BackgroundJobs;

/// <summary>
/// Refreshes every stored IP from IP2C at startup and then every <see cref="IpAddressRefreshOptions.Interval"/>,
/// one batch at a time.
/// </summary>
/// <remarks>
/// <see cref="PeriodicTimer"/> never overlaps cycles: if a cycle takes longer than the interval,
/// the next one starts as soon as it ends. Each batch runs in its own DI scope, so it gets a fresh DbContext
/// and tracked entities do not pile up across batches.
/// </remarks>
internal sealed class IpAddressRefreshJob(
    IServiceScopeFactory scopeFactory,
    IOptions<IpAddressRefreshOptions> options,
    ILogger<IpAddressRefreshJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);

        try
        {
            do
            {
                await RunCycleSafelyAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The application is shutting down.
        }
    }

    private async Task RunCycleSafelyAsync(CancellationToken stoppingToken)
    {
        // Like a request, each cycle gets its own correlation ID, so its log lines can be told apart.
        using var scope = logger.BeginScope("CorrelationId: {CorrelationId}", Guid.NewGuid().ToString("N"));

        try
        {
            await RunCycleAsync(stoppingToken);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            // An unhandled exception would stop the whole application; log it and retry next cycle instead.
            logger.LogError(exception, "IP refresh cycle failed; it will be retried in {Interval}.", options.Value.Interval);
        }
    }

    /// <summary>
    /// Runs one full cycle: every batch until the last one.
    /// </summary>
    internal async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        var batchSize = options.Value.BatchSize;
        var afterId = 0;
        int processed = 0, changed = 0, removed = 0, failed = 0;

        logger.LogInformation("IP refresh cycle started.");

        while (true)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var result = await sender.Send(new RefreshIpAddressBatchCommand(afterId, batchSize), cancellationToken);
            if (result.IsFailure)
            {
                logger.LogError("IP refresh cycle stopped: {ErrorCode} {ErrorDescription}", result.Error.Code, result.Error.Description);
                return;
            }

            var batch = result.Value;
            processed += batch.Processed;
            changed += batch.Changed;
            removed += batch.Removed;
            failed += batch.FailedIps.Count;

            if (batch.FailedIps.Count > 0)
            {
                logger.LogWarning(
                    "IP2C failed for {FailedCount} IPs; they are retried next cycle: {FailedIps}",
                    batch.FailedIps.Count,
                    batch.FailedIps);
            }

            if (batch.Processed < batchSize)
            {
                break;
            }

            afterId = batch.LastId;
        }

        logger.LogInformation(
            "IP refresh cycle finished: {Processed} processed, {Changed} changed, {Removed} removed, {Failed} failed.",
            processed,
            changed,
            removed,
            failed);
    }
}
