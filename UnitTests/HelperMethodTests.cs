using DataAccess.Data;
using DataAccess.Entities;
using IpInformation.Helpers;
using Microsoft.EntityFrameworkCore;

namespace UnitTests
{
    [CollectionDefinition("Database Tests", DisableParallelization = true)]
    public class HelperMethodTests
    {
        private readonly IpInformationDbContext _dbContext;
        private readonly List<IPAddresses> _ipInfoList;
        private readonly List<Countries> _countryList;

        public HelperMethodTests()
        {
            // Set up the in-memory database
            var options = new DbContextOptionsBuilder<IpInformationDbContext>()
                .UseInMemoryDatabase("TestDatabase") // Name of the in-memory database
                .Options;

            _dbContext = new IpInformationDbContext(options); // Use in-memory DbContext

            // Initialize test data
            _ipInfoList = new List<IPAddresses>
            {
                new IPAddresses { IP = "192.168.1.1", CountryId = 1 },
                new IPAddresses { IP = "192.168.1.2", CountryId = 2 }
            };

            _countryList = new List<Countries>
            {
                new Countries { ID = 1, Name = "Country1", TwoLetterCode = "C1", ThreeLetterCode = "C1A" },
                new Countries { ID = 2, Name = "Country2", TwoLetterCode = "C2", ThreeLetterCode = "C2B" }
            };

            // Add the test data to the in-memory database
            _dbContext.IPAddresses.AddRange(_ipInfoList);
            _dbContext.Countries.AddRange(_countryList);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task CreateCountryWithIPList_ReturnsListOfCountryWithIp()
        {
            // Act
            var result = await HelperMethods.CreateCountryWithIPList(_dbContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("192.168.1.1", result[0].IP);
            Assert.Equal("Country1", result[0].CountryName);
            Assert.Equal("C1", result[0].TwoLetterCode);
            Assert.Equal("C1A", result[0].ThreeLetterCode);
        }
        
        
        [Fact]
        public async Task CreateCountryWithIPList_ThrowsException_WhenCountryNotFound()
        {
            // Arrange: Add an IP with an invalid country ID to the database
            _ipInfoList.Add(new IPAddresses { IP = "192.168.1.3", CountryId = 99 });
            _dbContext.IPAddresses.AddRange(_ipInfoList);
            _dbContext.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => HelperMethods.CreateCountryWithIPList(_dbContext));
        }

        [Fact]
        public async Task CreateCountryWithIPList_ReturnsEmptyList_WhenNoIpsExist()
        {
            // Arrange: Empty IP list in the database
            _dbContext.IPAddresses.RemoveRange(_ipInfoList);
            _dbContext.SaveChanges();

            // Act
            var result = await HelperMethods.CreateCountryWithIPList(_dbContext);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        
    }
}
