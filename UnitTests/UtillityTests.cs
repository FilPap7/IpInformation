namespace UnitTests
{
    public class UtillityTests
    {

        [Theory]
        [InlineData("1.1.1.1", true)]
        [InlineData("a.b.c.d", false)]
        public void TestIpValidator(string ip, bool expectedResult)
        {
            // Act
            var testedIp = IpValidator.IsValidIp(ip);

            // Assert
            Assert.Equal(expectedResult, testedIp);
        }
    }
}