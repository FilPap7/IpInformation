using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpInformation.Helpers;
using DataAccess.Entities;
using DataAccess.Data;
using Common.Cache;

namespace IpInformation.Controllers
{
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IpInformationDbContext _dbContext;
        private readonly ICacheService _cacheContext;

        public IpController(IpInformationDbContext context, ICacheService cache)
        {
            _dbContext = context;
            _cacheContext = cache;
        }

        [HttpGet]
        [Route("Ip/GetAllIp")]
        public async Task<IActionResult> GetAllIpAddresses()
        {
            var ipAddresses = await _dbContext.IPAddresses.ToListAsync();

            return Ok(ipAddresses);
        }

        [HttpGet]
        [Route("Ip/GetAllCountryWithIpInfo")]
        public async Task<IActionResult> GetAllInfo()
        {
            var countriesWithIp = await HelperMethods.CreateCountryWithIPList(_dbContext);

            return Ok(countriesWithIp);
        }

        [HttpPost]
        [Route("Ip")]
        public async Task<IActionResult> GetIpInformation([FromBody] string Ip)
        {
            string ipCacheKey = $"IpInfo-{Ip}";
            string countryCacheKey = $"CountryInfo-{Ip}";

            var ipAddress = _cacheContext.GetData<IPAddresses>(ipCacheKey);
            var country = _cacheContext.GetData<Countries>(countryCacheKey);

              
            if (ipAddress == null)
            {
                ipAddress = await _dbContext.IPAddresses.FirstOrDefaultAsync(x => x.IP == Ip);

                if (ipAddress == null)
                {
                    // Get the Country from Ip2c Corresponding to the ID
                    if (country == null)
                    {
                        country = await Ip2c.GetIpInfo(Ip);

                        //check if country already exists in the DB
                        var tempCountry = await _dbContext.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode);

                        if (tempCountry != null)
                        {
                            country = tempCountry;
                        }
                        else
                        {
                            // Add the Country to the Countries table
                            _dbContext.Countries.Add(country);
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    ipAddress = new IPAddresses
                    {
                        IP = Ip,
                        CountryId = country.ID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Add the IP to the IPAddresses table
                    _dbContext.IPAddresses.Add(ipAddress);
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (country == null)
            {
                country = await _dbContext.Countries.FirstOrDefaultAsync(x => x.ID == ipAddress.CountryId);

                // If the Country does not exist in the Countries table, add it to the DB and the Cache
                if (country == null)
                {
                    // Get the Country from Ip2c Corresponding to the ID
                    country = await Ip2c.GetIpInfo(Ip);

                    // Add the Country to the Countries table
                    _dbContext.Countries.Add(country);
                    await _dbContext.SaveChangesAsync();
                }
            }

            // Ip and Country have values now so Update the cache
            _cacheContext.SetData(ipCacheKey, ipAddress, TimeSpan.FromMinutes(1));
            _cacheContext.SetData(countryCacheKey, country, TimeSpan.FromMinutes(1));

            return Ok(country);
        }
    }
}
