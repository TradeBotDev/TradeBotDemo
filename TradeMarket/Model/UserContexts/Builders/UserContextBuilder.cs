using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;
namespace TradeMarket.Model.UserContexts.Builders
{
    public class UserContextBuilder
    {
        protected UserContext context;

        public UserContext Result
        {
            get
            {
                UserContext result = context;
                Reset();
                return result;
            }
        }

        public UserContextBuilder()
        {
            context = new UserContext();
        }

        public UserContextBuilder AddUniqueInformation(string sessionId,string slotName)
        {
            context.Signature.SessionId = sessionId;
            context.Signature.SlotName = slotName;
            return this;
        }

        public UserContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            context.TradeMarket = tradeMarket;
            context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }

        public UserContextBuilder AddKeySecret(string key,string secret)
        {
            context.Key = key;
            context.Secret = secret;
            return this;
        }

        public UserContextBuilder AddWebSocketClient(BitmexWebsocketClient wsClient)
        {
            context.WSClient = wsClient;
            return this;
        }

        public UserContextBuilder InitUser()
        {
            //TODO тут явно что-то не так ...
            context.AutheticateUser(new System.Threading.CancellationToken());
            return this;
        }


        public void Reset()
        {
            context = new UserContext();
        }
    }
}
