using System.ComponentModel.DataAnnotations;

namespace WebFridgeApp.Models
{
    public class LoginData
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
