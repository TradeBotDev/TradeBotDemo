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

        public LoggedAccount() { }

        public LoggedAccount(Account account, bool saveExchangesAfterLogout)
        {
            this.AccountId = account.AccountId;
            this.SaveExchangesAfterLogout = saveExchangesAfterLogout;
        }
    }
}
