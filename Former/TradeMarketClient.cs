using Grpc.Core;
using Grpc.Net.Client;

using Serilog;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;

namespace Former
{
    public class TradeMarketClient
    {
        //"https://localhost:5005"
        private string _connectionString;

        private static TradeMarketClient _tradeMarketClient;

        private readonly Dictionary<string, double> _successfulOrders;
        private readonly TradeMarketService.TradeMarketServiceClient _client;

        public static TradeMarketClient GetInstance()
        {
            return _tradeMarketClient ??= new TradeMarketClient();
        }

        public void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        private TradeMarketClient()
        {
            _successfulOrders = new Dictionary<string, double>();
            var tradeMarketChannel = GrpcChannel.ForAddress(_connectionString);
            _client = new TradeMarketService.TradeMarketServiceClient(tradeMarketChannel);
        }

        public async void ObserveActualOrders()
        {
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };

            var request = new SubscribeOrdersRequest
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest
                {
                    Signature = orderSignature
                }
            };

            using var call = _client.SubscribeOrders(request);
            while (await call.ResponseStream.MoveNext())
            {
                Former.UpdateCurrentOrders(call.ResponseStream.Current);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }

        public async Task SendShoppingList(Dictionary<string, double> shoppingList)
        {
            _successfulOrders.Clear();
            foreach (var order in shoppingList)
            {
                var response = await _client.CloseOrderAsync(new CloseOrderRequest { Id = order.Key });
                Log.Debug("Requested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _successfulOrders.Add(order.Key, order.Value);
                    Console.Write(" ...purchased");
                }
                else Console.Write(" ...not purchased");
            }
            PlaceSuccessfulOrders();
        }

        private async void PlaceSuccessfulOrders()
        {
            foreach (var (key, value) in _successfulOrders)
            {
                var response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = value, Value = 2 });
                Log.Debug("Place order {0}", key);
                Log.Debug(response.Response.Code == ReplyCode.Succeed
                    ? " ...order placed"
                    : " ...order not placed");
            }
        }
    }
}
