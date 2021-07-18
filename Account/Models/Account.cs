using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public string Email { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public int PhoneNumber { get; set; }

        public string Password { get; set; }
    }
}
