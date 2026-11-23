using IpInformation.Application.Behaviors;
using IpInformation.Application.Tests.Fakes;
using IpInformation.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IpInformation.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private sealed record TestRequest;

    private readonly FakeLogger<LoggingBehavior<TestRequest, Result<int>>> _logger = new();
    private readonly LoggingBehavior<TestRequest, Result<int>> _behavior;

    public LoggingBehaviorTests()
    {
        _behavior = new LoggingBehavior<TestRequest, Result<int>>(_logger, TimeProvider.System);
    }

    private Task<Result<int>> HandleAsync(Result<int> response) =>
        _behavior.Handle(new TestRequest(), _ => Task.FromResult(response), CancellationToken.None);

    [Fact]
    public async Task Handle_WhenTheRequestSucceeds_LogsInformationWithTheRequestNameAndTime()
    {
        var response = await HandleAsync(Result.Success(42));

        Assert.Equal(42, response.Value);
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal(nameof(TestRequest), entry.Properties["RequestName"]);
        Assert.True(entry.Properties.ContainsKey("ElapsedMs"));
    }

    [Fact]
    public async Task Handle_WhenTheRequestFails_LogsAWarningWithTheErrorCode()
    {
        var error = Error.NotFound("Test.NotFound", "Nothing here.");

        var response = await HandleAsync(Result.Failure<int>(error));

        Assert.Same(error, response.Error);
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(nameof(TestRequest), entry.Properties["RequestName"]);
        Assert.Equal("Test.NotFound", entry.Properties["ErrorCode"]);
    }

    [Fact]
    public void AddApplication_Always_RegistersLoggingBeforeValidation()
    {
        var behaviors = new ServiceCollection().AddApplication()
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        Assert.Equal([typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)], behaviors);
    }

    [Fact]
    public async Task Handle_WhenTheHandlerThrows_LetsTheExceptionThroughWithoutLogging()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(new TestRequest(), _ => throw new InvalidOperationException(), CancellationToken.None));

        Assert.Empty(_logger.Entries);
    }
}
