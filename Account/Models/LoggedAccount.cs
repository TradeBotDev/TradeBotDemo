using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public class LoggedAccount
    {
        public int AccountId { get; set; }

        public bool SaveExchangesAfterLogout { get; set; }

        public LoggedAccount(Account account)
        {
            AccountId = account.AccountId;
        }
    }
}
