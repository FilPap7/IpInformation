using IpInformation.Application.Abstractions.Ip2c;

namespace IpInformation.Infrastructure.Ip2c;

/// <summary>
/// Calls <c>GET {BaseAddress}{ip}</c>. IP2C answers with plain text, <c>status;twoLetter;threeLetter;name</c>:
/// <list type="bullet">
/// <item><c>1;US;USA;United States of America (the)</c>: found.</item>
/// <item><c>2;...</c>: IP unknown to IP2C.</item>
/// <item><c>0;;;WRONG INPUT</c> (with HTTP 404): invalid IP.</item>
/// </list>
/// </summary>
internal sealed class Ip2cClient(HttpClient httpClient) : IIp2cClient
{
    public async Task<Ip2cCountry?> LookupAsync(string ip, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(Uri.EscapeDataString(ip), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        // The status is read from the body: IP2C uses HTTP 404 for "wrong input" as well.
        var parts = body.Trim().Split(';');

        if (parts is ["1", var twoLetterCode, var threeLetterCode, var name] && response.IsSuccessStatusCode)
        {
            return new Ip2cCountry(name, twoLetterCode, threeLetterCode);
        }

        if (parts[0] == "2")
        {
            return null;
        }

        throw new HttpRequestException(
            $"Unexpected IP2C response for IP {ip}: HTTP {(int)response.StatusCode}, body '{body}'.",
            inner: null,
            response.StatusCode);
    }
}
