using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.Model.Publishers;

namespace TradeMarket.Model.UserContexts
{
    public class CommonContext : ContextBase
    {
        public TradeMarket.Model.TradeMarkets.TradeMarket TradeMarket { get; internal set; }

        public async Task<List<BookLevel>> SubscribeToBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, CancellationToken token)
        {
            return await TradeMarket.SubscribeToBook25(handler, this, token);
        }
        public async Task UnSubscribeFromBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromBook25(handler,this);
        }

        public async Task<List<Instrument>> SubscribeToInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, CancellationToken token)
        {
            return await TradeMarket.SubscribeToInstruments(handler, this, token);
        }

        public async Task UnSubscribeFromInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler) {
            await TradeMarket.UnSubscribeFromInstruments(handler,this);
        }

        public CommonContext(IContext context) : base(context) { }

        public CommonContext() : base() { }

        public CommonContext(TradeMarket.Model.TradeMarkets.TradeMarket tradeMarket,string slotName) 
            : base(new ContextSignature(slotName,tradeMarket.Name,null))
        {
            TradeMarket = tradeMarket;
        }



        
    }
}
