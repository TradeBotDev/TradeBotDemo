using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class WrongKeySecretException : Exception
    {
        public WrongKeySecretException(string message) : base(message)
        {
        }
    }
}
