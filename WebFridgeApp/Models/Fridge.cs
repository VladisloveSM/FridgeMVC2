using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFridgeApp.Models
{
    public class Fridge
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string OwnerName { get; set; }

        public Guid ModelId { get; set; }

    }
}
