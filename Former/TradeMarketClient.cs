using Grpc.Core;
using Grpc.Net.Client;

using Serilog;

using System.Collections.Generic;
using System.Threading.Tasks;

using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Former
{
    public class TradeMarketClient
    {
        public delegate void CurrentPurchaseOrdersEvent(SubscribeOrdersResponse PurchaseOrdersToUpdate);
        public CurrentPurchaseOrdersEvent UpdatePurchaseOrders;

        public delegate void SuccessOrdersEvent(List<SubscribeOrdersResponse> successOrdersToUpdate);
        public SuccessOrdersEvent SellSuccessOrders;

        public delegate void BalanceEvent(Balance balanceToUpdate);
        public BalanceEvent UpdateBalance;

        private static string _connectionString;
        private static TradeMarketClient _tradeMarketClient;

        private readonly TradeMarketService.TradeMarketServiceClient _client;
        private readonly GrpcChannel _tradeMarketChannel;

        public static TradeMarketClient GetInstance()
        {
            if (_tradeMarketClient == null)
            {
                _tradeMarketClient = new TradeMarketClient();
            }
            return _tradeMarketClient;
        }

        public static void Configure(string connectionString)
        {
            _connectionString = connectionString;
        }

        private TradeMarketClient()
        {
            _tradeMarketChannel = GrpcChannel.ForAddress(_connectionString);
            _client = new TradeMarketService.TradeMarketServiceClient(_tradeMarketChannel);
        }

        public async void ObserveCurrentPurchaseOrders()
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
                UpdatePurchaseOrders?.Invoke(call.ResponseStream.Current);
            }
            _tradeMarketChannel.Dispose();
            //TODO выход из цикла и дальнейшее закрытие канала
        }

        public async Task SendShoppingList(List<SubscribeOrdersResponse> shoppingList)
        {
            List<SubscribeOrdersResponse> succesfullyPurchasedOrders = new List<SubscribeOrdersResponse>();
            foreach (var order in shoppingList)
            {
                var response = await _client.CloseOrderAsync(new CloseOrderRequest { Id = order.Response.Order.Id });
                Log.Debug("Requested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    succesfullyPurchasedOrders.Add(order);
                    Log.Debug(" ...purchased");
                }
                else Log.Debug(" ...not purchased");
            }
            SellSuccessOrders?.Invoke(succesfullyPurchasedOrders);
        }

        public async void PlaceSuccessfulOrders(List<SalesOrder> ordersForSale)
        {
            foreach (var order in ordersForSale)
            {
                var response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = order.price, Value = order.value });
                Log.Debug("Place order: price {0}, value {1}", order.price, order.value);
                Log.Debug(response.Response.Code == ReplyCode.Succeed
                    ? " ...order placed"
                    : " ...order not placed");
            }
        }

        public async void ObserveBalance()
        {
            var request = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest
            {
                Request = new TradeBot.Common.v1.SubscribeBalanceRequest()
            };
            using var call = _client.SubscribeBalance(request);
            while (await call.ResponseStream.MoveNext())
            {
                UpdateBalance?.Invoke(new Balance { bal1 = call.ResponseStream.Current.BalanceOne.Value, bal2 = call.ResponseStream.Current.BalanceTwo.Value});
            }
            _tradeMarketChannel.Dispose();
        }

        private async void ObserveMyOrders(List<Order> myOrders)
        {
            using var call = _client.SubscribyMyOrders();


            var responseReaderTask = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var note = call.ResponseStream.Current;
                }
            });
            //foreach (var order in _successfulOrders)
            //{
            //    await call.RequestStream.WriteAsync(new SubscribyMyOrdersRequest{ OrderId = order.Key});
            //}
            await call.RequestStream.CompleteAsync();
            await responseReaderTask;

        }

    }
}
