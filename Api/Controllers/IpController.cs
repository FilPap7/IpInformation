using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpInformation.Helpers;
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

        [HttpPost]
        [Route("Ip")]
        public async Task<IActionResult> GetIpInformation([FromBody] string Ip)
        {
            if (!IpValidator.IsValidIp(Ip)) return BadRequest("An Invalid IP was Given");

            var country = await HelperMethods.FindSingleIp(_dbContext, _cacheContext, Ip);

            return Ok(country);
        }

        [HttpGet]
        [Route("Ip/GetAllIp")]
        public async Task<IActionResult> GetAllIpAddresses()
        {
            var ipAddresses = await _dbContext.IPAddresses.ToListAsync();

            return Ok(ipAddresses);
        }

        [HttpPost]
        [Route("Ip/GetAllIp")]
        public async Task<IActionResult> GetAllIpAddresses([FromBody] int pageSize, [FromBody] string continuationToken)
        {
            if (pageSize <= 0)
            {
                return BadRequest("PageSize must be greater than 0.");
            }

            var ipAddresses = await HelperMethods.GetAllIpPaging(_dbContext, pageSize, continuationToken);

            return Ok(ipAddresses);
        }

        [HttpGet]
        [Route("Ip/GetAllCountryWithIpInfo")]
        public async Task<IActionResult> GetAllInfo()
        {
            var countriesWithIp = await HelperMethods.CreateCountryWithIPList(_dbContext);

            return Ok(countriesWithIp);
        }

        [HttpGet]
        [Route("Ip/UpdateDatabase")]
        public async Task<IActionResult> UpdateDatabase()
        {
            var success = await HelperMethods.UpdateDatabaseAsync(_dbContext, _cacheContext);

            if (success)
            {
                return Ok();
            }

            return BadRequest("DataBase Update resulted in Error");
        }
    }
}
