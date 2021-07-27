﻿using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
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

        private readonly TradeMarketService.TradeMarketServiceClient _client;
        private readonly GrpcChannel _channel;

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

        public async Task ObserveOrderBook(UserContext context)
        {
            var request = new SubscribeOrdersRequest
            {
                Request = new TradeBot.Common.v1.SubscribeOrdersRequest
                {
                    Signature = new OrderSignature
                    {
                        Status = OrderStatus.Open,
                        Type = OrderType.Sell
                    }
                }
            };

            using var call = _client.SubscribeOrders(request, context.Meta);

            Func<Task> observeCurrentPurchaseOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateOrderBook?.Invoke(call.ResponseStream.Current.Response.Order);
                }
            };

            await ConnectionTester(observeCurrentPurchaseOrders);
        }

        public async Task ObserveBalance(UserContext context)
        {
            var request = new TradeBot.TradeMarket.TradeMarketService.v1.SubscribeBalanceRequest
            {
                Request = new TradeBot.Common.v1.SubscribeBalanceRequest()
            };
            using var call = _client.SubscribeBalance(request, context.Meta);

            Func<Task> observeBalance = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateBalance?.Invoke(call.ResponseStream.Current.Response.Balance);
                }
            };

            await ConnectionTester(observeBalance);
        }

        public async Task ObserveMyOrders(UserContext context)
        {
            using var call = _client.SubscribeMyOrders(new SubscribeMyOrdersRequest(), context.Meta);
            Func<Task> observeMyOrders = async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateMyOrders?.Invoke(call.ResponseStream.Current.Changed);
                }
            };
            await ConnectionTester(observeMyOrders);
        }

        public async Task PlaceOrder(double sellPrice, double contractValue, UserContext context)
        {
            Log.Information("Order price: {0}, quantity: {1} placed", sellPrice, contractValue);
            PlaceOrderResponse response = null;
            Func<Task> placeOrders = async () =>
            {
                response = await _client.PlaceOrderAsync(new PlaceOrderRequest { Price = sellPrice, Value = contractValue }, context.Meta);
                Log.Information(response.OrderId + " placed " + response.Response.Code.ToString());
            };

            await ConnectionTester(placeOrders);
        }

        public async Task SetNewPrice(Order orderNeededToUpdate, UserContext context)
        {
            Log.Debug("Update order id: {0}, new price: {1}", orderNeededToUpdate.Id, orderNeededToUpdate.Price);
            AmmendOrderResponse response = null;
            Func<Task> placeOrders = async () =>
            {
                response = await _client.AmmendOrderAsync(new AmmendOrderRequest
                {
                    Id = orderNeededToUpdate.Id,
                    NewPrice = orderNeededToUpdate.Price,
                    QuantityType = QuantityType.None,
                    PriceType = PriceType.Default
                }, context.Meta);
                Log.Information(response.Response.Code.ToString());
            };
            await ConnectionTester(placeOrders);
        }
    }
}
