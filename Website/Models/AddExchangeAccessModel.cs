using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class AddExchangeAccessModel
    {
        [Required]
        public string SelectExchange { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Secret { get; set; }
    }
}
