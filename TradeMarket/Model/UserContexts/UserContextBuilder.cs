using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;
namespace TradeMarket.Model.UserContexts
{
    public class UserContextBuilder
    {
        protected UserContext context;

        public UserContextBuilder()
        {
            context = new UserContext();
        }

        public UserContextBuilder AddUniqueInformation(string sessionId,string slotName)
        {
            context.SessionId = sessionId;
            context.SlotName = slotName;
            return this;
        }

        public UserContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            context.TradeMarket = tradeMarket;
            return this;
        }

        public UserContextBuilder AddKeySecret(string key,string secret)
        {
            context.AssignKeySecret(key, secret);
            return this;
        }

        public UserContextBuilder AddWebSocketClient(BitmexWebsocketClient wsClient)
        {
            context.WSClient = wsClient;
            return this;
        }

        public UserContextBuilder AddRestfulClient(BitmexRestfulClient restClient)
        {
            context.RestClient = restClient;
            return this;
        }

        public UserContextBuilder InitUser()
        {
            context.init();
            return this;
        }


        public void Reset()
        {
            context = new UserContext();
        }
        public UserContext GetResult()
        {
            UserContext result = context;
            Reset();
            return result;
        }
    }
}
