using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class CommonContextBuilder 
    {
        private CommonContext Context;
        public CommonContext Result
        {
            get
            {
                CommonContext result = Context;
                return result;
            }
        }

        public CommonContextBuilder(ContextBuilder builder){
            Context = new CommonContext(builder.Context);
        }

        public CommonContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            Context.TradeMarket = tradeMarket;
            Context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }
    }
}
