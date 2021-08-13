using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model.UserContexts.Builders
{
    public class CommonContextBuilder
    {
        protected CommonContext context;

        public CommonContext Result
        {
            get
            {
                CommonContext result = context;
                Reset();
                return result;
            }
        }

        public CommonContextBuilder()
        {
            context = new CommonContext();
        }

        public CommonContextBuilder AddUniqueInformation(string slotName)
        {
            context.Signature.SessionId = null;
            context.Signature.SlotName = slotName;
            return this;
        }

        public CommonContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            context.TradeMarket = tradeMarket;
            context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }


        public void Reset()
        {
            context = new CommonContext();
        }
    }
}
