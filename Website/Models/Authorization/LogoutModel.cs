using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models.Authorization
{
    public class LogoutModel
    {
        [Required]
        public bool SaveExchanges { get; set; }

        [Required]
        public string Button { get; set; }

        public string PreviousUrl { get; set; }
    }
}
