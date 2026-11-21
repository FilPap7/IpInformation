using IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;
using IpInformation.Domain.Abstractions;
using IpInformation.Infrastructure.BackgroundJobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IpInformation.IntegrationTests.BackgroundJobs;

public class IpAddressRefreshJobTests
{
    /// <summary>
    /// Answers each batch command with the next prepared result and records the commands it received.
    /// </summary>
    private sealed class ScriptedSender(Queue<Result<RefreshIpAddressBatchResult>> results) : ISender
    {
        public List<RefreshIpAddressBatchCommand> Commands { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Commands.Add((RefreshIpAddressBatchCommand)(object)request);
            return Task.FromResult((TResponse)(object)results.Dequeue());
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private static RefreshIpAddressBatchResult Batch(int processed, int lastId) => new(processed, lastId, 0, 0, []);

    private static (IpAddressRefreshJob Job, ScriptedSender Sender) CreateJob(
        int batchSize, params Result<RefreshIpAddressBatchResult>[] results)
    {
        var sender = new ScriptedSender(new Queue<Result<RefreshIpAddressBatchResult>>(results));
        var scopeFactory = new ServiceCollection()
            .AddSingleton<ISender>(sender)
            .BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        var job = new IpAddressRefreshJob(
            scopeFactory,
            Options.Create(new IpAddressRefreshOptions { BatchSize = batchSize }),
            NullLogger<IpAddressRefreshJob>.Instance);

        return (job, sender);
    }

    [Fact]
    public async Task RunCycleAsync_WhenBatchesAreFull_ContinuesAfterTheLastIdUntilAShortBatch()
    {
        var (job, sender) = CreateJob(
            batchSize: 2,
            Result.Success(Batch(processed: 2, lastId: 5)),
            Result.Success(Batch(processed: 2, lastId: 9)),
            Result.Success(Batch(processed: 1, lastId: 12)));

        await job.RunCycleAsync(CancellationToken.None);

        Assert.Equal(
            [new(0, 2), new(5, 2), new(9, 2)],
            sender.Commands);
    }

    [Fact]
    public async Task RunCycleAsync_WhenThereAreNoIps_SendsOneBatchAndStops()
    {
        var (job, sender) = CreateJob(batchSize: 100, Result.Success(Batch(processed: 0, lastId: 0)));

        await job.RunCycleAsync(CancellationToken.None);

        Assert.Single(sender.Commands);
    }

    [Fact]
    public async Task RunCycleAsync_WhenABatchFails_StopsTheCycle()
    {
        var (job, sender) = CreateJob(
            batchSize: 2,
            Result.Failure<RefreshIpAddressBatchResult>(Error.Validation("Test.Invalid", "Invalid batch.")));

        await job.RunCycleAsync(CancellationToken.None);

        Assert.Single(sender.Commands);
    }
}
