using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class CreditCardModel
    {
        [Required]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        [Range(4, 4)]
        public int Date { get; set; }

        [Required]
        [Range(3, 3)]
        public int CVV { get; set; }
    }
}
