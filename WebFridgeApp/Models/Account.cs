﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebFridgeApp.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        public string? RefreshToken { get; set; }
    }
}
