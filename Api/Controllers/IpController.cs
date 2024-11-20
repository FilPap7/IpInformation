using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IpInformation.Helpers;
using DataAccess.Data;
using Common.Cache;
using System.Text.Json;

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
        public async Task<IActionResult> GetAllIpAddresses([FromBody] JsonElement payload)
        {
            // Set Default Values if body is empty

            int take = 100;
            string continuationToken = string.Empty;

            if (payload.TryGetProperty("take", out var idProperty))
            {
                take = idProperty.GetInt32();
            }

            if (payload.TryGetProperty("continuationToken", out var nameProperty))
            {
                continuationToken = nameProperty.GetString() ?? string.Empty;
            }

            if (take <= 0)
            {
                return BadRequest("PageSize must be greater than 0.");
            }

            var ipAddresses = await HelperMethods.GetAllIpPaging(_dbContext, take, continuationToken);

            return Ok(ipAddresses);
        }

        [HttpPost]
        [Route("Ip/GetAllBundledInfo")]
        public async Task<IActionResult> GetAllInfo([FromBody] JsonElement payload)
        {

            // Set Default Values if body is empty

            int take = 100;
            string continuationToken = string.Empty;

            if (payload.TryGetProperty("take", out var idProperty))
            {
                take = idProperty.GetInt32();
            }

            if (payload.TryGetProperty("continuationToken", out var nameProperty))
            {
                continuationToken = nameProperty.GetString() ?? string.Empty;
            }

            if (take <= 0)
            {
                return BadRequest("PageSize must be greater than 0.");
            }

            var countriesWithIpPaged = await HelperMethods.CreateCountryWithIPList(_dbContext, take, continuationToken);

            return Ok(countriesWithIpPaged);
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
