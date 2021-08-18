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

        public async Task SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToUserPositions(handler, this, token);
        }
        public async Task SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToUserMargin(handler, this, token);
        }
        public async Task SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToUserOrders(handler, this, token);
        }
        public async Task SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, CancellationToken token)
        {
            await TradeMarket.SubscribeToBalance(handler, this, token);
        }

 

        public async Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromUserPositions(handler, this);
        }
        public async Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromUserMargin(handler, this);
        }
        public async Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromUserOrders(handler, this);
        }
        public async Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler)
        {
            await TradeMarket.UnSubscribeFromBalance(handler, this);
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

        public async Task<BitmexResfulResponse<Order>> PlaceOrder(double quontity, double price,CancellationToken token)
        {
            return await TradeMarket.PlaceOrder(quontity, price, this,token);
        }

        public async Task<BitmexResfulResponse<Order[]>> DeleteOrder(string id, CancellationToken token)
        {
            return await TradeMarket.DeleteOrder(id, this,token);
        }

        public async Task<BitmexResfulResponse<Order>> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, CancellationToken token)
        {
            return await TradeMarket.AmmendOrder(id, price, Quantity, LeavesQuantity, this,token);
        }

        public async Task<bool> AutheticateUser(CancellationToken token)
        {
            bool result = await TradeMarket.AutheticateUser(this,token);
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

