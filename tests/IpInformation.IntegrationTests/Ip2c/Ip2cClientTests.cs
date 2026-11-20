using System.Net;
using IpInformation.Application.Abstractions.Ip2c;
using IpInformation.Infrastructure.Ip2c;

namespace IpInformation.IntegrationTests.Ip2c;

public class Ip2cClientTests
{
    /// <summary>
    /// Answers every request with a fixed status and body, and records the requested URI.
    /// </summary>
    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? RequestedUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestedUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(body) });
        }
    }

    private static (Ip2cClient Client, StubHandler Handler) Create(HttpStatusCode statusCode, string body)
    {
        var handler = new StubHandler(statusCode, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://ip2c.test/") };
        return (new Ip2cClient(httpClient), handler);
    }

    [Fact]
    public async Task LookupAsync_Always_RequestsTheIpUnderTheBaseAddress()
    {
        var (client, handler) = Create(HttpStatusCode.OK, "1;US;USA;United States of America (the)");

        await client.LookupAsync("8.8.8.8", CancellationToken.None);

        Assert.Equal(new Uri("https://ip2c.test/8.8.8.8"), handler.RequestedUri);
    }

    [Theory]
    [InlineData("1;US;USA;United States of America (the)", "United States of America (the)", "US", "USA")]
    [InlineData("1;XZ;XZZ;IANA Special-Purpose Address", "IANA Special-Purpose Address", "XZ", "XZZ")]
    [InlineData("1;GR;GRC;Greece\n", "Greece", "GR", "GRC")]
    public async Task LookupAsync_WhenFound_ReturnsTheCountry(string body, string name, string twoLetterCode, string threeLetterCode)
    {
        var (client, _) = Create(HttpStatusCode.OK, body);

        var country = await client.LookupAsync("8.8.8.8", CancellationToken.None);

        Assert.Equal(new Ip2cCountry(name, twoLetterCode, threeLetterCode), country);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("2;;;UNKNOWN")]
    public async Task LookupAsync_WhenIpIsUnknown_ReturnsNull(string body)
    {
        var (client, _) = Create(HttpStatusCode.OK, body);

        Assert.Null(await client.LookupAsync("8.8.8.8", CancellationToken.None));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "0;;;WRONG INPUT")]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.OK, "<html>maintenance</html>")]
    [InlineData(HttpStatusCode.OK, "1;US;USA")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "1;US;USA;United States")]
    public async Task LookupAsync_WhenResponseIsUnexpected_ThrowsHttpRequestException(HttpStatusCode statusCode, string body)
    {
        var (client, _) = Create(statusCode, body);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.LookupAsync("8.8.8.8", CancellationToken.None));
    }
}
