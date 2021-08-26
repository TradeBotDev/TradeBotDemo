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
        private BitmexContext _context;
        public BitmexContext Result
        {
            get
            {
                BitmexContext result = _context;
                return result;
            }
        }

        public BitmexContextBuilder(ContextBuilder builder)
        {
            _context = new BitmexContext(builder.Context);
        }

       

        public BitmexContextBuilder AddWebSocketClient(BitmexWebsocketClient wsClient)
        {
            _context.WSClient = wsClient;
            return this;
        }

        public async Task<BitmexContext> InitUserAsync(CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexContextBuilder>().ForContext("Method", nameof(InitUserAsync));
            if (await _context.AutheticateUserAsync(token, logger) == false)
            {
                throw new WrongKeySecretException($"{_context.Signature.SessionId} contains not real key secret");
            }
            return Result;
        }

        public void Reset()
        {
            _context = new BitmexContext();
        }
    }
}
