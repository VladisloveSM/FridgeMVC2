using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFridgeApp.Models
{
    public class FridgeProduct
    {
        public Guid Id { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public Guid ProductId { get; set; }

        public Guid FridgeId { get; set; }
    }
}
