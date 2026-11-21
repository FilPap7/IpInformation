using IpInformation.Domain.Abstractions;
using MediatR;

namespace IpInformation.Application.IpAddresses.Commands.RefreshIpAddressBatch;

/// <summary>
/// Refreshes one batch of stored IPs from IP2C: the IPs with <c>Id</c> greater than <paramref name="AfterId"/>,
/// at most <paramref name="BatchSize"/> of them. Used by the hourly refresh job, one batch per call.
/// </summary>
public sealed record RefreshIpAddressBatchCommand(int AfterId, int BatchSize)
    : IRequest<Result<RefreshIpAddressBatchResult>>;

/// <param name="Processed">How many IPs the batch contained; fewer than the batch size means it was the last one.</param>
/// <param name="LastId">The <c>Id</c> of the last IP in the batch; the next batch starts after it.</param>
/// <param name="Changed">IPs that moved to another country.</param>
/// <param name="Removed">IPs that IP2C no longer knows, so they were deleted.</param>
/// <param name="FailedIps">IPs skipped because IP2C failed or returned invalid data; they are retried next cycle.</param>
public sealed record RefreshIpAddressBatchResult(
    int Processed,
    int LastId,
    int Changed,
    int Removed,
    IReadOnlyList<string> FailedIps);
