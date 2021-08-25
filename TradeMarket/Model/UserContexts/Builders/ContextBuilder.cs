using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class ContextBuilder
    {
        private Context _context;
        internal Context Context { 
            get
            {
                var result = _context.Clone() as Context;
                _context = new Context();
                return result;
            }
            set
            {
                _context = value;
            }
        }

        public ContextBuilder() { Context = new Context(); }

        public ContextBuilder AddUniqueInformation(string slotName, string sessionId)
        {
            _context.Signature.SessionId = sessionId;
            _context.Signature.SlotName = slotName;
            return this;
        }
        public ContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            Context.TradeMarket = tradeMarket;
            Context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }

        public ContextBuilder AddKeySecret(string key, string secret)
        {
            Context.Key = key;
            Context.Secret = secret;
            return this;
        }

       
    }
}
