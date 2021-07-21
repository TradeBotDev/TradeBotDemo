using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public delegate void PurchaseOrdersEvent(Order purchaseOrdersToUpdate);
        public PurchaseOrdersEvent UpdatePurchaseOrders;

        public delegate void SellOrdersEvent(Dictionary<string, Order> successOrdersToUpdate);
        public SellOrdersEvent SellListUpdated;

        public delegate void BalanceEvent(Balance balanceToUpdate);
        public BalanceEvent UpdateBalance;

        public delegate void MyOrdersEvent(Order myOrderToUpdate);
        public MyOrdersEvent UpdateMyOrders;

        private static int _retryDelay;
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

        public static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        private TradeMarketClient()
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

            Func<Task> observeCurrentPurchaseOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdatePurchaseOrders?.Invoke(call.ResponseStream.Current.Response.Order);
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
            using var call = _client.SubscribeBalance(request);

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
            using var call = _client.SubscribyMyOrders(new SubscribyMyOrdersRequest());

            Func<Task> observeMyOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateMyOrders?.Invoke(call.ResponseStream.Current.Changed);
                }
            };

            await ConnectionTester(observeMyOrders);
        }

        public async Task CloseOrders(Dictionary<string, Order> preparedForPurchase, double contractValue)
        {
            CloseOrderResponse response = null;
            Func<Task> closeOrders;
            var succesfullyPurchasedOrders = new Dictionary<string, Order>();

            foreach (var order in preparedForPurchase)
            {
                closeOrders = async () =>
                {
                    response = await _client.CloseOrderAsync(new CloseOrderRequest { Id = order.Value.Id, Value = contractValue });
                };

                await ConnectionTester(closeOrders);

                Log.Debug("Requested to buy {0}", order);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    succesfullyPurchasedOrders.Add(order.Key, order.Value);
                    Log.Debug(" ...purchased");
                }
                else Log.Debug(" ...not purchased");
            }
            SellListUpdated?.Invoke(succesfullyPurchasedOrders);
        }

        public async Task PlaceSuccessfulOrders(List<SalesOrder> ordersForSale)
        {
            PlaceOrderResponse response = null;
            Func<Task> placeSuccessfulOrders;
            foreach (var order in ordersForSale)
            {
                placeSuccessfulOrders = async () =>
                {
                    response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = order.price, Value = order.value });
                };

                await ConnectionTester(placeSuccessfulOrders);
                
                Log.Debug("Place order: price {0}, value {1}", order.price, order.value);
                Log.Debug(response.Response.Code == ReplyCode.Succeed
                    ? " ...order placed"
                    : " ...order not placed");
            }
        }

        public async Task UpdateMyOrdersOnTM(Dictionary<string, double> orderToUpdate) 
        {
            
        
        }
    }
}
