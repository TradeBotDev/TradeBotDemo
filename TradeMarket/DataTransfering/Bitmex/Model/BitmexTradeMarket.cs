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
using TradeMarket.Model.UserContexts.Builders;
using TradeMarket.Model;
using Bitmex.Client.Websocket;

namespace TradeMarket.DataTransfering.Bitmex.Model
{
    public class BitmexTradeMarket : TradeMarket.Model.TradeMarkets.TradeMarket
    {
        
        public BitmexTradeMarket() : base() { }

        public BitmexTradeMarket(string name, BitmexPublisherFactory factory) : base(name, factory) { }


        #region RestRequests



        public async override Task<ResfulResponse<Order>> AmmendOrderAsync(string id, double? price, long? Quantity, long? LeavesQuantity, Context context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(AmmendOrderAsync));
            return await CommonRestClient.SendAsync(new AmmendOrderRequest(context.Key, context.Secret, new() { Id = id, LeavesQuantity = LeavesQuantity, Price = price, Quantity = Quantity }), token);
        }

        public async override Task<ResfulResponse<Order[]>> DeleteOrderAsync(string id, Context context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(DeleteOrderAsync));
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

        public async override Task<ResfulResponse<Order>> PlaceOrderAsync(double quontity, double price, Context context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(PlaceOrderAsync));
            return await CommonRestClient.SendAsync(new PlaceOrderRequest(context.Key, context.Secret, new Order
            {
                Symbol = context.Signature.SlotName,
                OrdType = "Limit",
                Price = RoundToHalfOrZero(price),
                OrderQty = (long?)quontity
            }), token);

        }
        #endregion

        public async override Task<bool> AutheticateUserAsync(Context context, CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(AutheticateUserAsync));
            var complition = new TaskCompletionSource<bool>();
            EventHandler<IPublisher<bool>.ChangedEventArgs> handler = (sender, args) => complition.SetResult(args.Changed);
            await SubscribeToAsync(AuthenticationPublisher, handler, context, PublisherFactory.CreateAuthenticationPublisher, token,log);
            bool result = await complition.Task;
            await UnsubscribeFromAsync(AuthenticationPublisher, context, handler,log);
            return result;
        }

        public override async Task<Context> BuildContextAsync(ContextBuilder builder,CancellationToken token, ILogger logger)
        {
            var log = logger.ForContext<BitmexTradeMarket>().ForContext("Method", nameof(BuildContextAsync));
            var contextBuilder = new  BitmexContextBuilder (builder);
            contextBuilder.AddWebSocketClient(ClientsFactory.CreateWebsocketClient(BitmexValues.ApiWebsocketTestnetUrl));
            return await contextBuilder.InitUserAsync(token, logger);
    }
    }
}

