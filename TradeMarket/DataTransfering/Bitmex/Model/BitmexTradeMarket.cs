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

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarket : TradeMarket.Model.TradeMarkets.TradeMarket
    {
        public BitmexTradeMarket() : base() { }

        public BitmexTradeMarket(string name, IPublisherFactory factory) : base(name, factory) { }

        #region Subscribe Requests

        #region Common 
        public async override Task SubscribeToInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler, IContext context, CancellationToken token)
        {
            if (InstrumentPublisher is null)
            {
                InstrumentPublisher = await CreatePublisher(CommonWSClient,handler, context, token, PublisherFactory.CreateInstrumentPublisher);
            }
            await InstrumentPublisher.Start();
        }

        public async override Task SubscribeToBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler, IContext context, CancellationToken token)
        {
            if (Book25Publisher is null)
            {
                Book25Publisher = await CreatePublisher(CommonWSClient, handler, context, token, PublisherFactory.CreateBook25Publisher);
            }
            await Book25Publisher.Start();
        }
        #endregion

        #region User

        public async override Task SubscribeToUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context, CancellationToken token)
        {
            if (PositionPublisher.ContainsKey(context) == false)
            {
                PositionPublisher[context] = await CreatePublisher(context.WSClient,handler, context, token, PublisherFactory.CreateUserPositionPublisher);
            }
            await PositionPublisher[context].Start();
        }


        public async override Task SubscribeToUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context, CancellationToken token)
        {
            if (MarginPublisher.ContainsKey(context) == false)
            {
                MarginPublisher[context] = await CreatePublisher(context.WSClient,handler, context, token, PublisherFactory.CreateUserMarginPublisher);
            }
            await MarginPublisher[context].Start();
        }

        

        public async override Task SubscribeToUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context, CancellationToken token)
        {
            if (OrderPublisher.ContainsKey(context) == false)
            {
                OrderPublisher[context] = await CreatePublisher(context.WSClient,handler, context, token, PublisherFactory.CreateUserOrderPublisher);
            }
            await OrderPublisher[context].Start();
        }

        public async override Task SubscribeToBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context, CancellationToken token)
        {
            if (WalletPublishers.ContainsKey(context) == false)
            {
                WalletPublishers[context] = await CreatePublisher(context.WSClient,handler, context, token, PublisherFactory.CreateWalletPublisher);
            }
            await WalletPublishers[context].Start();
        }
        #endregion

        #endregion

        #region UnSubscribe Requests
        public async override Task UnSubscribeFromInstruments(EventHandler<IPublisher<Instrument>.ChangedEventArgs> handler)
        {
            await UnsubscribeFrom(InstrumentPublisher, handler);
        }

        public async override Task UnSubscribeFromBook25(EventHandler<IPublisher<BookLevel>.ChangedEventArgs> handler)
        {
            await UnsubscribeFrom(Book25Publisher, handler);
        }

        public async override Task UnSubscribeFromUserPositions(EventHandler<IPublisher<Position>.ChangedEventArgs> handler, UserContext context)
        {
            await UnsubscribeFrom(PositionPublisher, context, handler);
        }

        public async override Task UnSubscribeFromUserMargin(EventHandler<IPublisher<Margin>.ChangedEventArgs> handler, UserContext context)
        {
            await UnsubscribeFrom(MarginPublisher, context, handler);
        }

        public async override Task UnSubscribeFromUserOrders(EventHandler<IPublisher<Order>.ChangedEventArgs> handler, UserContext context)
        {
            await UnsubscribeFrom(OrderPublisher, context, handler);
        }

        public async override Task UnSubscribeFromBalance(EventHandler<IPublisher<Wallet>.ChangedEventArgs> handler, UserContext context)
        {
            await UnsubscribeFrom(WalletPublishers, context, handler);
        }



        #endregion


        #region RestRequests

       

        public async override Task<BitmexResfulResponse<Order>> AmmendOrder(string id, double? price, long? Quantity, long? LeavesQuantity, IContext context,CancellationToken token)
        {
            return await CommonRestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() { Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), token);
        }

        public async override Task<BitmexResfulResponse<Order>> DeleteOrder(string id, IContext context,CancellationToken token)
        {
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

        public async override Task<BitmexResfulResponse<Order>> PlaceOrder(double quontity, double price, IContext context,CancellationToken token)
        {
            return await CommonRestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
            {
                Symbol = context.Signature.SlotName,
                OrdType = "Limit",
                Price = RoundToHalfOrZero(price),
                OrderQty = (long?)quontity
            }), token);
           
        }
        #endregion

        public async override Task<bool> AutheticateUser(UserContext context, CancellationToken token)
        {
            var complition = new TaskCompletionSource<bool>();
            EventHandler<IPublisher<bool>.ChangedEventArgs> handler = (sender, args) => complition.SetResult(args.Changed);
            
            if (AuthenticationPublisher.ContainsKey(context) == false)
            {
                AuthenticationPublisher[context] = await CreatePublisher(context.WSClient,handler, context, token, PublisherFactory.CreateAuthenticationPublisher);
                AuthenticationPublisher[context].Changed += handler;
            }
            await AuthenticationPublisher[context].Start();

            bool result = await complition.Task;
            AuthenticationPublisher[context].Changed -= handler;
            return result;
        }
        
    }
}

