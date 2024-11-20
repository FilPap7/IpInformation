using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace ApiModels.DTO
{
    public class CountryWithIpPaged
    {
        public required List<CountryWithIp> CountryWithIp { get; set; }

        public string ContinuationToken { get; set; } = string.Empty;

    }
}
