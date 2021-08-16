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

        //вот эти ивенты нужны ли вообще ?
        public event EventHandler<IPublisher<BookLevel>.ChangedEventArgs> Book25;
        public event EventHandler<IPublisher<Instrument>.ChangedEventArgs> InstrumentUpdate;

        public async Task SubscribeToBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToBook25(handler, this, token);
        }
        public async Task UnSubscribeFromBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromBook25(handler);
        }

        public async Task SubscribeToInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToInstruments(handler, this, token);
        }

        public async Task UnSubscribeFromInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler) {
            await TradeMarket.UnSubscribeFromInstruments(handler);
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
