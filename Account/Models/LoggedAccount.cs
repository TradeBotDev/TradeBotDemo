using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public class LoggedAccount
    {
        public Account AccountInfo { get; set; }

        public List<ExchangeAccess> Exchanges;

        public LoggedAccount(Account account)
        {
            AccountInfo = account;
            Exchanges = new List<ExchangeAccess>();
        }
    }
}
