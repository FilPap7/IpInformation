using Microsoft.Extensions.Logging;

namespace IpInformation.Application.Tests.Fakes;

/// <summary>
/// Records every log entry with its structured properties, so tests can check what was logged.
/// </summary>
internal sealed class FakeLogger<T> : ILogger<T>
{
    public sealed record Entry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Properties);

    public List<Entry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var properties = state as IEnumerable<KeyValuePair<string, object?>> ?? [];
        Entries.Add(new Entry(logLevel, formatter(state, exception), properties.ToDictionary(p => p.Key, p => p.Value)));
    }
}
