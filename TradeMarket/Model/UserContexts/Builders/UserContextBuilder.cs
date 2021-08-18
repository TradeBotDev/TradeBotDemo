using Bitmex.Client.Websocket.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;
namespace TradeMarket.Model.UserContexts.Builders
{
    public class UserContextBuilder
    {
        private UserContext Context;
        public UserContext Result
        {
            get
            {
                UserContext result = Context;
                Reset();
                return result;
            }
        }

        public UserContextBuilder(ContextBuilder builder)
        {
            Context = new UserContext(builder.Context);
        }

        public UserContextBuilder AddTradeMarket(TradeMarkets.TradeMarket tradeMarket)
        {
            Context.TradeMarket = tradeMarket;
            Context.Signature.TradeMarketName = tradeMarket.Name;
            return this;
        }

        public UserContextBuilder AddKeySecret(string key,string secret)
        {
            Context.Key = key;
            Context.Secret = secret;
            return this;
        }

        public UserContextBuilder AddWebSocketClient(BitmexWebsocketClient wsClient)
        {
            Context.WSClient = wsClient;
            return this;
        }

        public async Task<UserContext> InitUser(CancellationToken token)
        {
            if(await Context.AutheticateUser(token) == false)
            {
                throw new WrongKeySecretException($"{Context.Signature.SessionId} contains not real key secret");
            }
            return Result;
        }

        public void Reset()
        {
            Context = new UserContext();
        }
    }
}
