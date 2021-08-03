using Grpc.Core;
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
        public delegate void MyOrdersEvent(Order newComingOrder, ChangesType changesType);
        public MyOrdersEvent UpdateMyOrders;

        public delegate void BalanceEvent(int balanceToBuy, int balanceToSell);
        public BalanceEvent UpdateBalance;

        public delegate void PositionUpdate(double currentQuantity);
        public PositionUpdate UpdatePosition;

        public delegate void MarketPricesUpdate(double bid, double ask);
        public MarketPricesUpdate UpdateMarketPrices;

        private static int _retryDelay;
        private static string _connectionString;

        private readonly TradeMarketService.TradeMarketServiceClient _client;

        public static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        public TradeMarketClient()
        {
            _client = new TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress(_connectionString));
        }

        /// <summary>
        /// Проверяет соединение с биржей, на вход принимает функцию, осуществляющую общение с биржей
        /// </summary>
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
                    Log.Error("Error {1}. Retrying...\r\n{0}", e.StackTrace);
                    Thread.Sleep(_retryDelay);
                }
            }
        }

        public async Task ObserveMarketPrices(UserContext context)
        {
            using var call = _client.SubscribePrice(new SubscribePriceRequest(), context.Meta);

            async Task ObserveMarketPricesFunc()
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateMarketPrices?.Invoke(call.ResponseStream.Current.BidPrice,call.ResponseStream.Current.AskPrice);
                }
            }

            await ConnectionTester(ObserveMarketPricesFunc);
        }

        /// <summary>
        /// Наблюдает за обновлением доступного баланса 
        /// </summary>
        public async Task ObserveBalance(UserContext context)
        {
            using var call = _client.SubscribeMargin(new SubscribeMarginRequest(), context.Meta);

            async Task ObserveBalanceFunc()
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdateBalance?.Invoke((int) call.ResponseStream.Current.Margin.AvailableMargin, (int)call.ResponseStream.Current.Margin.MarginBalance);
                }
            }

            await ConnectionTester(ObserveBalanceFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих ордеров
        /// </summary>
        public async Task ObserveMyOrders(UserContext context)
        {
            using var call = _client.SubscribeMyOrders(new SubscribeMyOrdersRequest(), context.Meta);

            async Task ObserveMyOrdersFunc()
            {
                while (await call.ResponseStream.MoveNext())
                {
                    if (call.ResponseStream.Current.Response.Code == ReplyCode.Failure)
                    {
                        Log.Information("order was rejected with message: {0}", call.ResponseStream.Current.Response.Message);
                        continue;
                    }

                    UpdateMyOrders?.Invoke(call.ResponseStream.Current.Changed, call.ResponseStream.Current.ChangesType);
                }
            }

            await ConnectionTester(ObserveMyOrdersFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих позиций
        /// </summary>
        public async Task ObservePositions(UserContext context)
        {
            using var call = _client.SubscribePosition(new SubscribePositionRequest(), context.Meta);

            async Task ObservePositionFunc()
            {
                while (await call.ResponseStream.MoveNext())
                {
                    UpdatePosition?.Invoke(call.ResponseStream.Current.CurrentQty);
                }
            }

            await ConnectionTester(ObservePositionFunc);
        }

        /// <summary>
        /// Отправляет запрос в биржу на выставление своего ордера
        /// </summary>
        public async Task<PlaceOrderResponse> PlaceOrder(double sellPrice, double contractValue, UserContext context)
        {
            PlaceOrderResponse response = null;

            async Task PlaceOrdersFunc()
            {
                response = await _client.PlaceOrderAsync(new PlaceOrderRequest {Price = sellPrice, Value = contractValue}, context.Meta);
            }

            await ConnectionTester(PlaceOrdersFunc);
            return response;
        }

        /// <summary>
        /// Отправляет запрос в биржу на изменение цены своего ордера
        /// </summary>
        public async Task<AmmendOrderResponse> AmendOrder(string id, double newPrice, UserContext context)
        {
            AmmendOrderResponse response = null;

            async Task PlaceOrdersFunc()
            {
                response = await _client.AmmendOrderAsync(new AmmendOrderRequest
                {
                    Id = id,
                    NewPrice = newPrice,
                    QuantityType = QuantityType.None,
                    NewQuantity = 0,
                    PriceType = PriceType.Default
                }, context.Meta);
            }

            await ConnectionTester(PlaceOrdersFunc);
            return response;
        }
    }
}
