using System.ComponentModel.DataAnnotations;

namespace ApiModels.DTO
{
    // This is a Class that Represents a Controller Response
    // It will take part in the Database
    public class CountryWithIp
    {
        [StringLength(15)] // Enforce maximum IP address length
        public string IP { get; set; } = string.Empty;

        [StringLength(50)]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        [StringLength(2)] // Enforce two-letter limit
        public string TwoLetterCode { get; set; } = string.Empty;

        [Required]
        [StringLength(3)] // Enforce three-letter limit
        public string ThreeLetterCode { get; set; } = string.Empty;

        public string ContinuationToken { get; set; } = string.Empty;
    }
}
