using IpInformation.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IpInformation.IntegrationTests.Api;

public class CorrelationIdMiddlewareTests
{
    /// <summary>
    /// DefaultHttpContext never "starts" the response; this feature runs the OnStarting callbacks on demand.
    /// </summary>
    private sealed class StartableResponseFeature : HttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStarting = [];

        public override void OnStarting(Func<object, Task> callback, object state) => _onStarting.Add((callback, state));

        public async Task StartAsync()
        {
            foreach (var (callback, state) in _onStarting)
            {
                await callback(state);
            }
        }
    }

    /// <summary>
    /// Records the scopes that are open, as text, e.g. "CorrelationId: abc".
    /// </summary>
    private sealed class ScopeRecordingLogger : ILogger<CorrelationIdMiddleware>
    {
        public List<string> Scopes { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            Scopes.Add(state.ToString()!);
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    private sealed record Outcome(string? SeenByNext, string? ResponseHeader, IReadOnlyList<string> Scopes, HttpContext Context);

    private static async Task<Outcome> InvokeAsync(params string[] requestHeaderValues)
    {
        var response = new StartableResponseFeature();
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(response);
        if (requestHeaderValues.Length > 0)
        {
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = requestHeaderValues;
        }

        string? seenByNext = null;
        var logger = new ScopeRecordingLogger();
        var middleware = new CorrelationIdMiddleware(
            ctx =>
            {
                seenByNext = CorrelationIdMiddleware.Get(ctx);
                return Task.CompletedTask;
            },
            logger);

        await middleware.InvokeAsync(context);
        await response.StartAsync();

        return new Outcome(seenByNext, context.Response.Headers[CorrelationIdMiddleware.HeaderName], logger.Scopes, context);
    }

    [Fact]
    public async Task InvokeAsync_WhenTheRequestHasNoId_CreatesOneAndReturnsItInTheResponseAndTheLogScope()
    {
        var outcome = await InvokeAsync();

        Assert.False(string.IsNullOrEmpty(outcome.SeenByNext));
        Assert.Equal(outcome.SeenByNext, outcome.ResponseHeader);
        Assert.Equal([$"CorrelationId: {outcome.SeenByNext}"], outcome.Scopes);
    }

    [Theory]
    [InlineData("abc-123")]
    [InlineData("0f8fad5b-d9cb-469f-a165-70867728950e")]
    [InlineData("order_42.retry")]
    public async Task InvokeAsync_WhenTheRequestHasASafeId_KeepsIt(string id)
    {
        var outcome = await InvokeAsync(id);

        Assert.Equal(id, outcome.SeenByNext);
        Assert.Equal(id, outcome.ResponseHeader);
    }

    public static TheoryData<string[]> UnsafeIds() => new()
    {
        new[] { "" },
        new[] { "has space" },
        new[] { "line\nbreak" },
        new[] { "<script>" },
        new[] { new string('a', 65) },
        new[] { "first", "second" },
    };

    [Theory]
    [MemberData(nameof(UnsafeIds))]
    public async Task InvokeAsync_WhenTheRequestIdIsUnsafe_ReplacesItWithANewOne(string[] headerValues)
    {
        var outcome = await InvokeAsync(headerValues);

        Assert.False(string.IsNullOrEmpty(outcome.SeenByNext));
        Assert.DoesNotContain(outcome.SeenByNext, headerValues);
        Assert.Equal(outcome.SeenByNext, outcome.ResponseHeader);
    }

    [Fact]
    public async Task AddToProblemDetails_DuringARequest_AddsTheCorrelationId()
    {
        var outcome = await InvokeAsync("abc-123");
        var problem = new ProblemDetailsContext { HttpContext = outcome.Context, ProblemDetails = new ProblemDetails() };

        CorrelationIdMiddleware.AddToProblemDetails(problem);

        Assert.Equal("abc-123", problem.ProblemDetails.Extensions["correlationId"]);
    }

    [Fact]
    public void AddToProblemDetails_OutsideTheMiddleware_AddsNothing()
    {
        var problem = new ProblemDetailsContext { HttpContext = new DefaultHttpContext(), ProblemDetails = new ProblemDetails() };

        CorrelationIdMiddleware.AddToProblemDetails(problem);

        Assert.False(problem.ProblemDetails.Extensions.ContainsKey("correlationId"));
    }
}
