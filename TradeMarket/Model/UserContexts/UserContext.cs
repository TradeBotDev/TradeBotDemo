using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Instruments;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Wallets;
using Bitmex.Client.Websocket.Websockets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using TradeMarket.Clients;
using TradeMarket.DataTransfering;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using TradeMarket.Model.Publishers;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;

namespace TradeMarket.Model.UserContexts
{
    public class UserContext : ContextBase
    {
        #region Dynamic Part

        public readonly TaskCompletionSource<UserContext> AutharizationCompleted = new TaskCompletionSource<UserContext>();

        public Model.TradeMarkets.TradeMarket TradeMarket { get; set; }

        public async Task<List<Position>> SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToUserPositions(handler, this, token,logger);
        }
        public async Task<List<Margin>> SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToUserMargin(handler, this, token,logger);
        }
        public async Task<List<Order>> SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToUserOrders(handler, this, token,logger);
        }
        public async Task<List<Wallet>> SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToBalance(handler, this, token,logger);
        }

        public async Task<List<BookLevel>> SubscribeToBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToBook25(handler, this, token,logger);
        }

        public async Task<List<Instrument>> SubscribeToInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.SubscribeToInstruments(handler, this, token,logger);
        }

        public async Task UnSubscribeFromBook25UpdatesAsync(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromBook25(handler, this,logger);
        }

        public async Task UnSubscribeFromInstrumentUpdate(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromInstruments(handler, this,logger);
        }


        public async Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromUserPositions(handler, this,logger);
        }
        public async Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromUserMargin(handler, this,logger);
        }
        public async Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromUserOrders(handler, this,logger);
        }
        public async Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, ILogger logger)
        {
            await TradeMarket.UnSubscribeFromBalance(handler, this,logger);
        }

        //Клиенты для доступа к личной информации пользователя на бирже
        internal BitmexWebsocketClient WSClient { get; set; }

        public UserContext(IContext context): base(context)
        {

        }

        public UserContext():base()
        {

        }

        internal UserContext(string sessionId, string slotName, Model.TradeMarkets.TradeMarket tradeMarket) 
            : base(new ContextSignature(slotName,tradeMarket.Name,sessionId))
        {
            TradeMarket = tradeMarket;
        }

        public async Task<BitmexResfulResponse<Order>> PlaceOrder(double quontity, double price,CancellationToken token, ILogger logger)
        {
            return await TradeMarket.PlaceOrder(quontity, price, this,token,logger);
        }

        public async Task<BitmexResfulResponse<Order[]>> DeleteOrder(string id, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.DeleteOrder(id, this,token,logger);
        }

        public async Task<BitmexResfulResponse<Order>> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, CancellationToken token, ILogger logger)
        {
            return await TradeMarket.AmmendOrder(id, price, Quantity, LeavesQuantity, this,token,logger);
        }

        public async Task<bool> AutheticateUser(CancellationToken token,ILogger logger)
        {
            bool result = await TradeMarket.AutheticateUser(this,token,logger);
            if(result == true)
            {
                AutharizationCompleted.SetResult(this);
            }
            else
            {
                AutharizationCompleted.SetException(new ArgumentException("Provided Key and Secret is not valid"));
            }
            return result;
        }

        #endregion


    }
}

