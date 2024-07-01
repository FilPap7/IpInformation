using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Helpers;
using DataAccess.Entities;
using Microsoft.Extensions.Caching.Memory;
using Common.Cache;
using System.Diagnostics.Metrics;
using System.Net;

namespace IpInformation.Controllers
{
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly DataAccess.Data.IpInformationDbContext _dbContext;
        private readonly ICacheService _cacheContext;
        private readonly string cacheKey = "IP";
        private readonly string cacheKey2 = "Country";

        public IpController(DataAccess.Data.IpInformationDbContext context, ICacheService cache)
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

        [HttpPost]
        [Route("Ip")]
        public async Task<IActionResult> GetIpInformation([FromBody] string Ip)
        {
            var ipAddress = _cacheContext.GetData<IPAddresses>(Ip);
            Countries country;

            if (ipAddress != null)
            {
                return Ok(await _dbContext.Countries.FirstOrDefaultAsync(x => x.ID == ipAddress.CountryId));
            }   
            else
            {
                ipAddress = await _dbContext.IPAddresses.FirstOrDefaultAsync(x => x.IP == Ip);

                if (ipAddress == null)
                {
                    // Get the Country from Ip2c Corresponding to the ID
                    country = await Ip2c.GetIpInfo(Ip);

                    var IpAddress = new IPAddresses
                    {
                        IP = Ip,
                        CountryId = country.ID,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // Add the IP Address to the IPAddresses table
                    _dbContext.IPAddresses.Add(IpAddress);

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    //Check if the Cache has the Country
                    country = _cacheContext.GetData<Countries>(Ip);

                    if (country == null)
                    {
                        // If the Country does not exist in the Countries table, add it to the DB and the Cache
                        if (await _dbContext.Countries.FirstOrDefaultAsync(x => x.ID == ipAddress.CountryId) == null)
                        {
                            // Get the Country from Ip2c Corresponding to the ID
                            country = await Ip2c.GetIpInfo(Ip);

                            // Add the Country to the Countries table
                            _dbContext.Countries.Add(country);

                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }

                // Update the cache
                _cacheContext.SetData(Ip, ipAddress, TimeSpan.FromMinutes(5));
                _cacheContext.SetData(cacheKey2, country, TimeSpan.FromMinutes(5));

                return Ok(country);
            }
        }
    }
}
