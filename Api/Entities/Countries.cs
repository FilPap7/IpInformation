using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace IpInformation.Entities
{
    public class Countries
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2)] // Enforce two-letter limit
        public string TwoLetterCode { get; set; } = string.Empty;

        [Required]
        [StringLength(3)] // Enforce three-letter limit
        public string ThreeLetterCode { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime2(7)")]  // Specify datetime2 for nanosecond precision
        public DateTime CreatedAt { get; set; }
    }
}
