using Bitmex.Client.Websocket.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering.Bitmex.Model;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts;
using TradeMarket.Model.UserContexts.Builders;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexContextBuilder
    {
        private BitmexContext Context;
        public BitmexContext Result
        {
            get
            {
                BitmexContext result = Context;
                Reset();
                return result;
            }
        }

        public BitmexContextBuilder(ContextBuilder builder)
        {
            Context = new BitmexContext(builder.Context);
        }

       

        public BitmexContextBuilder AddWebSocketClient(BitmexWebsocketClient wsClient)
        {
            Context.WSClient = wsClient;
            return this;
        }

        public async Task<BitmexContext> InitUserAsync(CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexContextBuilder>().ForContext("Method", nameof(InitUserAsync));
            if (await Context.AutheticateUser(token, logger) == false)
            {
                throw new WrongKeySecretException($"{Context.Signature.SessionId} contains not real key secret");
            }
            return Result;
        }

        public void Reset()
        {
            Context = new BitmexContext();
        }
    }
}
