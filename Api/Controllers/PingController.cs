using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IpInformation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok(true);
        }
    }
}
