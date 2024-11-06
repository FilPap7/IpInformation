using Microsoft.AspNetCore.Mvc;
using IpInformation.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

namespace IpInformation.Controllers
{
    public class HealthCheckController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly AppSettings _settings;

        public HealthCheckController(HealthCheckService healthCheckService, AppSettings settings)
        {
            _healthCheckService = healthCheckService;
            _settings = settings;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetHealthStatus()
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(new { status = "Healthy", details = healthReport });
            }

            return StatusCode(503, new { status = "Unhealthy", details = healthReport });
        }

        [HttpGet("ServiceStatus")]
        public async Task<IActionResult> CheckServices()
        {
            var countryHealth = await Ip2c.GetIpInfo(Constants.GoogleDNS);

            if (countryHealth == null)
            {
                return StatusCode(503, new { status = "IP Service is not Responding" });
            }

            try
            {
                using (var connection = new SqlConnection(_settings.ConnectionStrings.WorkConnectionString))
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = $"Database Connection is Unhealthy. Exception: {ex}" });
            }

            return Ok(new { status = $"Service is Healthy, " +
                "Services Checked: Database, IP Service" });


        }
    }
}
