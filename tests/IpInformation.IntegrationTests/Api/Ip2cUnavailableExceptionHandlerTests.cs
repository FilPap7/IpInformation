using System.Text.Json;
using IpInformation.Api.ExceptionHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace IpInformation.IntegrationTests.Api;

public class Ip2cUnavailableExceptionHandlerTests
{
    private static async Task<(bool Handled, HttpContext Context)> HandleAsync(Exception exception)
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();

        var handler = new Ip2cUnavailableExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(),
            NullLogger<Ip2cUnavailableExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        return (handled, context);
    }

    public static TheoryData<Exception> Ip2cFailures() => new()
    {
        new HttpRequestException("Connection refused."),
        new TaskCanceledException("Timed out.", new TimeoutException()),
    };

    [Theory]
    [MemberData(nameof(Ip2cFailures))]
    public async Task TryHandleAsync_WhenIp2cFails_Writes503ProblemWithCode(Exception exception)
    {
        var (handled, context) = await HandleAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(503, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(Ip2cUnavailableExceptionHandler.ErrorCode, json.RootElement.GetProperty("code").GetString());
    }

    public static TheoryData<Exception> OtherFailures() => new()
    {
        new InvalidOperationException("A bug."),
        new TaskCanceledException("The caller cancelled the request."),
    };

    [Theory]
    [MemberData(nameof(OtherFailures))]
    public async Task TryHandleAsync_WhenTheFailureIsNotFromIp2c_LeavesItToTheDefaultHandler(Exception exception)
    {
        var (handled, context) = await HandleAsync(exception);

        Assert.False(handled);
        Assert.Equal(0, context.Response.Body.Length);
    }
}
