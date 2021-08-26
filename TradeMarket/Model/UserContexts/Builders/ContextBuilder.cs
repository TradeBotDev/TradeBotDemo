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
                return result;
            }
            set
            {
                _context = value;
            }
        }

        public void Reset()
        {
            _context = new Context();

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
            _context.TradeMarket = tradeMarket;
            _context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }

        public ContextBuilder AddKeySecret(string key, string secret)
        {
            _context.Key = key;
            _context.Secret = secret;
            return this;
        }

       
    }
}
