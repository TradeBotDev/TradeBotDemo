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

        public delegate void BalanceEvent(TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceResponse balanceToUpdate);
        public BalanceEvent UpdateBalance;

        public delegate void MyOrdersEvent(SubscribyMyOrdersResponse myOrderToUpdate);
        public MyOrdersEvent UpdateMyOrders;

        private static string _connectionString;
        private static TradeMarketClient _tradeMarketClient;

        private readonly TradeMarketService.TradeMarketServiceClient _client;
        private readonly GrpcChannel _channel;

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
            _channel = GrpcChannel.ForAddress(_connectionString);
            _client = new TradeMarketService.TradeMarketServiceClient(_channel);
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
            while (true)
            {
                try
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdatePurchaseOrders?.Invoke(call.ResponseStream.Current);
                    }
                }
                catch (RpcException e)
                {
                    Thread.Sleep(10000);
                    Log.Debug("Exception in ObserveCurrentPurchaseOrders(). Retrying...\r\n{0}", e.Status.DebugException.Message);
                }
            }
            _channel.Dispose();
        }

        public async void ObserveBalance()
        {
            var request = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest
            {
                Request = new TradeBot.Common.v1.SubscribeBalanceRequest()
            };
            using var call = _client.SubscribeBalance(request);
            while (true)
            {
                try
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdateBalance?.Invoke(call.ResponseStream.Current);
                    }
                }
                catch (RpcException e)
                {
                    Thread.Sleep(10000);
                    Log.Debug("Exception in ObserveBalance(). Retrying...\r\n{0}", e.Status.DebugException.Message);
                }
            }
            _channel.Dispose();
        }

        public async void ObserveMyOrders()
        {
            using var call = _client.SubscribyMyOrders(new SubscribyMyOrdersRequest());
            while (true)
            {
                try
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        UpdateMyOrders?.Invoke(call.ResponseStream.Current);
                    }
                }
                catch (RpcException e)
                {
                    Thread.Sleep(10000);
                    Log.Debug("Exception in ObserveMyOrders(). Retrying...\r\n{0}", e.Status.DebugException.Message);
                }
            }
            _channel.Dispose();
        }

        public async Task CloseOrders(Dictionary<string, SubscribeOrdersResponse> preparedForPurchase, double contractValue)
        {
            CloseOrderResponse response;
            var succesfullyPurchasedOrders = new Dictionary<string, SubscribeOrdersResponse>();
            
            foreach (var order in preparedForPurchase)
            {
                while (true)
                {
                    try
                    {
                        response = await _client.CloseOrderAsync(new CloseOrderRequest { Id = order.Value.Response.Order.Id, Value = contractValue });
                        break;
                    }
                    catch (RpcException e)
                    {
                        Thread.Sleep(10000);
                        Log.Debug("Exception in CloseOrders(). Retrying...\r\n{0}", e.Status.DebugException.Message);
                    }
                }
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
            PlaceOrderResponse response;
            foreach (var order in ordersForSale)
            {
                while (true)
                {
                    try
                    {
                        response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = order.price, Value = order.value });
                        break;
                    }
                    catch (RpcException e)
                    {
                        Thread.Sleep(10000);
                        Log.Debug("Exception in PlaceSuccessfulOrders(). Retrying...\r\n{0}", e.Status.DebugException.Message);
                    }
                }
                Log.Debug("Place order: price {0}, value {1}", order.price, order.value);
                Log.Debug(response.Response.Code == ReplyCode.Succeed
                    ? " ...order placed"
                    : " ...order not placed");
            }
        }
    }
}
