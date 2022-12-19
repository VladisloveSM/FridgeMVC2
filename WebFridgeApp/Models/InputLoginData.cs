using System;
using System.ComponentModel.DataAnnotations;

namespace WebFridgeApp.Models
{
    public class InputLoginData
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string Repeat { get; set; }
    }
}
