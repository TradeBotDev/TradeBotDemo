using Grpc.Core;
using TradeBot.Common.v1;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Grpc.Net.Client;
using TradeBot.Algorithm.AlgorithmService.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;
using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;

namespace Former.Services
{
    public class TradeMarketClient
    {
        private static GrpcChannel TradeMarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
        private static TradeMarketService.TradeMarketServiceClient Client = new TradeMarketService.TradeMarketServiceClient(TradeMarketChannel);
        private static Dictionary<string, double> SuccessfulOrders = new();
        
        public static async void ObserveTradeMarket()
        {  
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };
            var request = new SubscribeOrdersRequest()
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest()
                {
                    Signature = orderSignature
                }

            };
            using var call = Client.SubscribeOrders(request);
            while (await call.ResponseStream.MoveNext())
            {
                Former.UpdateCurrentOrders(call.ResponseStream.Current);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }
        public static async Task SendShopingList(Dictionary<string, double> shoppingList)
        {
            SuccessfulOrders.Clear();
            CloseOrderResponse response;
            foreach (var order in shoppingList)
            {
                response = await Client.CloseOrderAsync(new CloseOrderRequest() { Id = order.Key });
                Console.Write("\nRequested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    SuccessfulOrders.Add(order.Key, order.Value);
                    Console.Write(" ...purchased");
                }
                else Console.Write(" ...not purchased");
            }
            PlaceSuccessfulOrders();
            //BeginObserveMyOrders();
        }
        public static async void PlaceSuccessfulOrders()
        {
            PlaceOrderResponse response;
            foreach (var order in SuccessfulOrders)
            {
                response = await Client.PlaceOrderAsync(new PlaceOrderRequest() { Price = order.Value, Value = 2 });
                Console.Write("\nPlace order {0}", order.Key);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    Console.Write(" ...order placed\n");
                }
                else Console.Write(" ...order not placed\n");
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
