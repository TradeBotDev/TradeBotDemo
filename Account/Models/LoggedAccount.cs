using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public class LoggedAccount
    {
        public Account AccountInfo { get; set; }

        public List<TradeMarketAccess> TradeMarkets;

        public LoggedAccount(Account account)
        {
            AccountInfo = account;
            TradeMarkets = new List<TradeMarketAccess>();
        }
    }
}
