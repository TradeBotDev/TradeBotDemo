using Grpc.Core;
using TradeBot.Former.FormerService.v1;
using TradeBot.Common.v1;
using SubscribeOrdersRequest = TradeBot.Former.FormerService.v1.SubscribeOrdersRequest;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Former.Services
{
    public class Observer
    {
        static List<string> SuccessfulOrders = new();
        public static async void ObserveAlgorithm()
        {
            var algorithmClient = new FormerService.FormerServiceClient(Channels.AlgorithmChannel);
            using var call = algorithmClient.SubscribePurchasePrice(new SubscribePurchasePriceRequest());

            while (await call.ResponseStream.MoveNext())
            {
                Former.FormShoppingList(call.ResponseStream.Current.PurchasePrice);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }
        public static async void ObserveTradeMarket()
        {
            var tradeMarketClient = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };
            var request = new SubscribeOrdersRequest()
            {
                Signature = orderSignature
            };

            using var call = tradeMarketClient.SubscribeOrders(request);
            while (await call.ResponseStream.MoveNext())
            {
                Former.UpdateCurrentOrders(call.ResponseStream.Current);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }
        public static async Task SendShopingList(List<string> shoppingList)
        {
            var client = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
            CloseOrderResponse response;
            foreach (var order in shoppingList)
            {
                response = await client.CloseOrderAsync(new CloseOrderRequest() { Id = order });
                Console.Write("\nRequested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    SuccessfulOrders.Add(order);
                    Console.Write(" ...purchased");
                }
                else Console.Write(" ...not purchased");
            }
            PlaceSuccessfulOrders();
            //BeginObserveMyOrders();
        }
        public static async void PlaceSuccessfulOrders()
        {
            var client = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
            PlaceOrderResponse response;
            foreach (var order in SuccessfulOrders)
            {
                response = await client.PlaceOrderAsync(new PlaceOrderRequest() { Price = 30, Value = 2 });
                Console.Write("\nPlace order {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    Console.Write(" ...order placed");
                }
                else Console.Write(" ...order not placed");
            }
        }


        //public static async void ObserveMyOrders(string id) 
        //{
        //    var tradeMarketClient = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
        //    var orderSignature = new OrderSignature
        //    {
        //        Status = OrderStatus.Open,
        //        Type = OrderType.Buy
        //    };
        //    var request = new SubscribeOrdersRequest()
        //    {
        //        Request = orderSignature
        //    };

        //    using var call = tradeMarketClient.SubscribeOrders(request);
        //    while (await call.ResponseStream.MoveNext())
        //    {
        //        Former.UpdateCurrentOrders(call.ResponseStream.Current);
        //    }
        //}
        //private async static Task BeginObserveMyOrders() 
        //{
        //    foreach (var order in OrdersForObserving) 
        //    {
        //        await ObserveMyOrders(order);
        //    }

        //}
    }
}
