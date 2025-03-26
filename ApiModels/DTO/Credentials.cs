using System.ComponentModel.DataAnnotations;

namespace ApiModels.DTO
{
    public class Credentials
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
