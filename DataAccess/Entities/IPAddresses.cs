using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class IPAddresses
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        [StringLength(15)] // Enforce maximum IP address length
        public string IP { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
