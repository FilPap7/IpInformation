using Common;

namespace IntegrationTests
{
    public class ExternalServiceTests
    {
        // Utilize Cloudflare's DNS as a Test IP
        [Theory]
        [InlineData("1.1.1.1")]
        public void TestIp2cService(string ip)
        {

            // Act
            var testedIp = Ip2c.GetIpInfo(ip);

            // Assert
            Assert.NotNull(testedIp);
        }
    }
}