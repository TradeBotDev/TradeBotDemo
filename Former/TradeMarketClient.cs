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

namespace Former
{
    public class TradeMarketClient
    {
        public delegate void OrderBookEvent(Order purchaseOrdersToUpdate);
        public OrderBookEvent UpdateOrderBook;

        public delegate void MyOrdersEvent(Order myOrderToUpdate);
        public MyOrdersEvent UpdateMyOrders;

        public delegate void BalanceEvent(Balance balanceToUpdate);
        public BalanceEvent UpdateBalance;

        private static int _retryDelay;
        private static string _connectionString;

        private Metadata _metadata;

        private readonly TradeMarketService.TradeMarketServiceClient _client;
        private readonly GrpcChannel _channel;

        public void SetMetadata(Metadata metadata) 
        {
            _metadata = metadata;
        }

        public static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        public TradeMarketClient()
        {
            _channel = GrpcChannel.ForAddress(_connectionString);
            _client = new TradeMarketService.TradeMarketServiceClient(_channel);
        }

        private async Task ConnectionTester(Func<Task> func)
        {
            while (true)
            {
                try
                {
                    await func.Invoke();
                    break;
                }
                catch (RpcException e)
                {
                    Log.Error("Error {1}. Retrying...\r\n{0}", e.Status.DebugException.Message, e.StackTrace);
                    Thread.Sleep(_retryDelay);
                }
            }
        }

        public async void ObserveOrderBook()
        {
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Sell
            };
            var request = new SubscribeOrdersRequest
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest
                {
                    Signature = orderSignature
                }
            };
            
            using var call = _client.SubscribeOrders(request, _metadata);

            Func<Task> observeCurrentPurchaseOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateOrderBook?.Invoke(call.ResponseStream.Current.Response.Order);
                }
            };

            await ConnectionTester(observeCurrentPurchaseOrders);
        }

        public async void ObserveBalance()
        {
            var request = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest
            {
                Request = new TradeBot.Common.v1.SubscribeBalanceRequest()
            };
            using var call = _client.SubscribeBalance(request, _metadata);

            Func<Task> observeBalance = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateBalance?.Invoke(call.ResponseStream.Current.Response.Balance);
                }
            };

            await ConnectionTester(observeBalance);
        }

        public async void ObserveMyOrders()
        {
            using var call = _client.SubscribeMyOrders(new SubscribeMyOrdersRequest(), _metadata);
            Func<Task> observeMyOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateMyOrders?.Invoke(call.ResponseStream.Current.Changed);
                }
            };
            await ConnectionTester(observeMyOrders);
        }

        public async Task PlacePurchaseOrders(Dictionary<double, double> purchaseList)
        {
            PlaceOrderResponse response = null;
            Func<Task> closeOrders;
            foreach (var order in purchaseList)
            {
                Log.Information("Order: price: {0}, quantity: {1} placed", order.Key, order.Value);
                closeOrders = async () =>
                {
                    response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = order.Key, Value = order.Value }, _metadata);
                };
                await ConnectionTester(closeOrders);
            }
        }

        public async Task PlaceSellOrder(double sellPrice, double contractValue)
        {
            Log.Information("Order: price: {0}, quantity: {1} placed", sellPrice, contractValue);
            PlaceOrderResponse response = null;
            Func<Task> placeSuccessfulOrders;
            placeSuccessfulOrders = async () =>
            {
                response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = sellPrice, Value = contractValue }, _metadata);
            };
            await ConnectionTester(placeSuccessfulOrders);
        }

        public async Task TellTMUpdateMyOreders(Dictionary<string, double> orderToUpdate)
        {
            foreach (var order in orderToUpdate)
            {
                Log.Debug("Update order id: {0}, new price: {1}", order.Key, order.Value);
            }

        }
    }
}
