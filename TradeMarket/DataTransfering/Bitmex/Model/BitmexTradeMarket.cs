using System;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using Serilog;
using Order = Bitmex.Client.Websocket.Responses.Orders.Order;
using Margin = Bitmex.Client.Websocket.Responses.Margins.Margin;
using DeleteOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.DeleteOrderRequest;
using PlaceOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Place.PlaceOrderRequest;
using AmmendOrderRequest = TradeMarket.DataTransfering.Bitmex.Rest.Requests.Ammend.AmmendOrderRequest;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Wallets;
using Bitmex.Client.Websocket.Responses.Instruments;
using TradeMarket.Model.Publishers;
using TradeMarket.Model.UserContexts;
using TradeMarket.DataTransfering.Bitmex.Rest.Responses;
using System.Threading;
using System.Collections.Generic;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarket : TradeMarket.Model.TradeMarkets.TradeMarket
    {
        public BitmexTradeMarket() : base() { }

        public BitmexTradeMarket(string name, IPublisherFactory factory) : base(name, factory) { }

        #region Subscribe Requests

        #region Common 
        public async override Task<List<Instrument>> SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, IContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToInstruments));
            return await SubscribeTo(CommonWSClient, InstrumentPublisher, handler, context,PublisherFactory.CreateInstrumentPublisher, token,log);
        }

        public async override Task<List<BookLevel>> SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, IContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToBook25));
            return await SubscribeTo(CommonWSClient, Book25Publisher, handler, context, PublisherFactory.CreateBook25Publisher, token,log);
        }
        #endregion

        #region User

        public async override Task<List<Position>> SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToUserPositions));
            return await SubscribeTo(context.WSClient, PositionPublisher, handler, context, PublisherFactory.CreateUserPositionPublisher, token,log);
        }


        public async override Task<List<Margin>> SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToUserMargin));
            return await SubscribeTo(context.WSClient, MarginPublisher, handler, context, PublisherFactory.CreateUserMarginPublisher, token,log);
        }



        public async override Task<List<Order>> SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToUserOrders));
            return await SubscribeTo(context.WSClient, OrderPublisher, handler, context, PublisherFactory.CreateUserOrderPublisher, token,log);
        }

        public async override Task<List<Wallet>> SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(SubscribeToBalance));
            return await SubscribeTo(context.WSClient, WalletPublishers, handler, context, PublisherFactory.CreateWalletPublisher, token,log);
        }
        #endregion

        #endregion

        #region UnSubscribe Requests
        public async override Task UnSubscribeFromInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(UnSubscribeFromInstruments));
            await UnsubscribeFrom(InstrumentPublisher,context, handler,log);
        }

        public async override Task UnSubscribeFromBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler,IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(UnSubscribeFromBook25));
            await UnsubscribeFrom(Book25Publisher,context, handler,log);
        }

        public async override Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(UnSubscribeFromUserPositions));
            await UnsubscribeFrom(PositionPublisher, context, handler,log);
        }

        public async override Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(UnSubscribeFromUserMargin));
            await UnsubscribeFrom(MarginPublisher, context, handler,log);
        }

        public async override Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(UnSubscribeFromUserOrders));
            await UnsubscribeFrom(OrderPublisher, context, handler,log);
        }

        public async override Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, IContext context, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method",nameof(UnSubscribeFromBalance));
            await UnsubscribeFrom(WalletPublishers, context, handler,log);
        }



        #endregion


        #region RestRequests

       

        public async override Task<BitmexResfulResponse<Order>> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, IContext context,CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(AmmendOrder));
            return await CommonRestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() { Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), token);
        }

        public async override Task<BitmexResfulResponse<Order[]>> DeleteOrder(string id, IContext context,CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(DeleteOrder));
            return await CommonRestClient.SendAsync(new DeleteOrderRequest(context.Key, context.Secret, id), token);
        }

        //TODO перенести в конвертер
        public static double RoundToHalfOrZero(double value)
        {
            double truncatedDifference = value - Math.Truncate(value);
            //если что-то случилось и пришло не ровно 0.5
            //разница с 0.5 больше чем 0.01 считается нарошной 
            if (Math.Abs(truncatedDifference - 0.5) < 0.01)
            {
                return Math.Truncate(value) + 0.5;
            }
            else
            {
                return Math.Truncate(value);
            }
        }

        public async override Task<BitmexResfulResponse<Order>> PlaceOrder(double quontity, double price, IContext context,CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(PlaceOrder));
            return await CommonRestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
            {
                Symbol = context.Signature.SlotName,
                OrdType = "Limit",
                Price = RoundToHalfOrZero(price),
                OrderQty = (long?)quontity
            }), token);
           
        }
        #endregion

        public async override Task<bool> AutheticateUser(UserContext context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(AutheticateUser));
            var complition = new TaskCompletionSource<bool>();
            EventHandler<IPublisher<bool>.ChangedEventArgs> handler = (sender, args) => complition.SetResult(args.Changed);
            await SubscribeTo(context.WSClient, AuthenticationPublisher, handler, context, PublisherFactory.CreateAuthenticationPublisher, token,log);
            bool result = await complition.Task;
            await UnsubscribeFrom(AuthenticationPublisher, context, handler,log);
            return result;
        }
        
    }
}

