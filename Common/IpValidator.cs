using System.Net;

public class IpValidator
{
    public static bool IsValidIp(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }
}