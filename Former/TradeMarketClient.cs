using Grpc.Core;
using Grpc.Net.Client;

using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

using SubscribeOrdersRequest = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersRequest;
using SubscribeOrdersResponse = TradeBot.TradeMarket.TradeMarketService.v1.SubscribeOrdersResponse;

namespace Former
{
    public class TradeMarketClient
    {
        public delegate void CurrentPurchaseOrdersEvent(SubscribeOrdersResponse purchaseOrdersToUpdate);
        public CurrentPurchaseOrdersEvent UpdatePurchaseOrders;

        public delegate void SuccessOrdersEvent(Dictionary<string, SubscribeOrdersResponse> successOrdersToUpdate);
        public SuccessOrdersEvent SellSuccessOrders;

        public delegate void BalanceEvent(Balance balanceToUpdate);
        public BalanceEvent UpdateBalance;

        public delegate void MyOrdersEvent(SubscribyMyOrdersResponse myOrderToUpdate);
        public MyOrdersEvent UpdateMyOrders;

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
            int attempts = 0;
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
            while (true)
            {
                try
                {
                    if (!(call.ResponseStream.Current is null)) attempts = 0;
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdatePurchaseOrders?.Invoke(call.ResponseStream.Current);
                    }
                }
                catch (Exception e)
                {
                    if (attempts >= 10) break;
                    attempts++;
                    Thread.Sleep(10000);
                    Log.Debug("Исключение в наблюдателе за актуальными ценами: {0}", e.Message);
                    Log.Debug("Повторная попытка... ");
                }
            }
            _tradeMarketChannel.Dispose();
            //TODO выход из цикла и дальнейшее закрытие канала
        }

        public async void ObserveBalance()
        {
            int attempts = 0;
            var request = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest
            {
                Request = new TradeBot.Common.v1.SubscribeBalanceRequest()
            };
            using var call = _client.SubscribeBalance(request);
            while (true)
            {
                try
                {
                    if (!(call.ResponseStream.Current is null)) attempts = 0;
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdateBalance?.Invoke(new Balance { bal1 = call.ResponseStream.Current.Response.Balance.Value, bal2 = call.ResponseStream.Current.Response.Balance.Value });
                    }
                }
                catch (Exception e)
                {
                    if (attempts >= 10) break;
                    attempts++;
                    Thread.Sleep(10000);
                    Log.Debug("Исключение в наблюдателе за балансом: {0}", e.Message);
                    Log.Debug("Повторная попытка... ");
                }
            }
            _tradeMarketChannel.Dispose();
        }

        public async void ObserveMyOrders()
        {
            int attempts = 0;
            using var call = _client.SubscribyMyOrders(new SubscribyMyOrdersRequest());
            while (true)
            {
                try
                {
                    if (!(call.ResponseStream.Current is null)) attempts = 0;
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdateMyOrders?.Invoke(call.ResponseStream.Current);
                    }
                }
                catch (Exception e)
                {
                    if (attempts >= 10) break;
                    attempts++;
                    Thread.Sleep(10000);
                    Log.Debug("Исключение в наблюдателе за своими ордерами: {0}", e.Message);
                    Log.Debug("\r\nПовторная попытка... ");
                }
            }
        }

        public async Task CloseOrders(Dictionary<string, SubscribeOrdersResponse> preparedForPurchase)
        {
            Dictionary<string, SubscribeOrdersResponse> succesfullyPurchasedOrders = new Dictionary<string, SubscribeOrdersResponse>();
            foreach (var order in preparedForPurchase)
            {
                var response = await _client.CloseOrderAsync(new CloseOrderRequest { Id = order.Value.Response.Order.Id });
                Log.Debug("Requested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    succesfullyPurchasedOrders.Add(order.Key, order.Value);
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
    }
}
