using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpInformation.Helpers;
using IpInformation.Entities;
using System.Diagnostics.Metrics;

namespace IpInformation.Controllers
{
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly Data.IpInformation _context;

        public IpController(Data.IpInformation context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Ip/GetAllIp")]
        public async Task<IActionResult> GetAllIpAddresses()
        {
            var ipAddresses = await _context.IPAddresses.ToListAsync();

            return Ok(ipAddresses);
        }

        [HttpPost]
        [Route("Ip")]
        public async Task<IActionResult> GetIpInformation([FromBody] string Ip)
        {
            var ipAddress = await _context.IPAddresses.FirstOrDefaultAsync(x => x.IP == Ip);
            var country = new Countries();

            if (ipAddress == null)
            {

                // Get the Country from Ip2c Corresponding to the ID
                country = await Ip2c.GetIpInfo(Ip);

                // If the Country does not exist in the Countries table, add it
                if (await _context.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode) == null)
                {

                    // Add the Country to the Countries table
                    _context.Countries.Add(country);
                    await _context.SaveChangesAsync();

                    // Add the IP Address to the IPAddresses table
                    _context.IPAddresses.Add(new IPAddresses 
                    { 
                        IP = Ip, CountryId = country.ID 
                    });
                    await _context.SaveChangesAsync();

                    //return the IP's country information
                    return Ok(await _context.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode));
                }
                else 
                {
                    //return the IP's country information
                    return Ok(await _context.Countries.FirstOrDefaultAsync(x => x.TwoLetterCode == country.TwoLetterCode));
                }   
            }
            else
            {
                //return the IP's country information
                return Ok(await _context.Countries.FirstOrDefaultAsync(x => x.ID == ipAddress.CountryId));
            }

            
        }
    }
}
