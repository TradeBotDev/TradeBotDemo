using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former.Clients
{
    public class TradeMarketClient
    {
        public delegate Task MyOrdersEvent(Order newComingOrder, ChangesType changesType);
        public MyOrdersEvent UpdateMyOrders;

        public delegate Task BalanceEvent(int availableBalance, int totalBalance);
        public BalanceEvent UpdateBalance;

        public delegate Task PositionEvent(double positionQuantity);
        public PositionEvent UpdatePosition;

        public delegate Task MarketPricesEvent(double bid, double ask);
        public MarketPricesEvent UpdateMarketPrices;

        private static int _retryDelay;
        private static string _connectionString;
        private CancellationTokenSource _token;

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
            var attempts = 0;
            while (true)
            {
                try
                {
                    await func.Invoke();
                    break;
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Cancelled || attempts > 3) break;
                    Log.Error("{@Where}: Error {@ExceptionMessage}. Retrying...\r\n{@ExceptionStackTrace}", "Former", e.Message, e.StackTrace);
                    Thread.Sleep(_retryDelay);
                    attempts++;
                }
            }
        }

        /// <summary>
        /// Наблиюдает за изменением рыночных цен
        /// </summary>
        private async Task ObserveMarketPrices(Metadata meta)
        {
            using var call = _client.SubscribePrice(new SubscribePriceRequest(), meta);
            async Task ObserveMarketPricesFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    await UpdateMarketPrices?.Invoke(call.ResponseStream.Current.BidPrice, call.ResponseStream.Current.AskPrice);
                }
            }

            await ConnectionTester(ObserveMarketPricesFunc);
        }

        /// <summary>
        /// Наблюдает за обновлением доступного баланса 
        /// </summary>
        private async Task ObserveBalance(Metadata meta)
        {
            using var call = _client.SubscribeMargin(new SubscribeMarginRequest(), meta);

            async Task ObserveBalanceFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    await UpdateBalance?.Invoke((int) call.ResponseStream.Current.Margin.AvailableMargin, (int)call.ResponseStream.Current.Margin.MarginBalance);
                }
            }
            await ConnectionTester(ObserveBalanceFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих ордеров
        /// </summary>
        private async Task ObserveMyOrders(Metadata meta)
        {
            using var call = _client.SubscribeMyOrders(new SubscribeMyOrdersRequest(), meta);

            async Task ObserveMyOrdersFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    await UpdateMyOrders?.Invoke(call.ResponseStream.Current.Changed, call.ResponseStream.Current.ChangesType);
                }
            }

            await ConnectionTester(ObserveMyOrdersFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих позиций
        /// </summary>
        private async Task ObservePositions(Metadata meta)
        {
            using var call = _client.SubscribePosition(new SubscribePositionRequest(), meta);

            async Task ObservePositionFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    await UpdatePosition?.Invoke(call.ResponseStream.Current.CurrentQty);
                }
            }

            await ConnectionTester(ObservePositionFunc);
        }

        /// <summary>
        /// Отправляет запрос в биржу на выставление своего ордера
        /// </summary>
        internal async Task<PlaceOrderResponse> PlaceOrder(double sellPrice, double contractValue, Metadata metadata)
        {
            PlaceOrderResponse response = null;

            async Task PlaceOrdersFunc()
            {
                response = await _client.PlaceOrderAsync(new PlaceOrderRequest {Price = sellPrice, Value = contractValue}, metadata);
            }

            await ConnectionTester(PlaceOrdersFunc);
            return response;
        }

        /// <summary>
        /// Отправляет запрос в биржу на изменение цены своего ордера
        /// </summary>
        internal async Task<AmmendOrderResponse> AmendOrder(string id, double newPrice, Metadata metadata)
        {
            AmmendOrderResponse response = null;

            async Task AmendOrdersFunc()
            {
                response = await _client.AmmendOrderAsync(new AmmendOrderRequest
                {
                    Id = id,
                    NewPrice = newPrice,
                    QuantityType = QuantityType.None,
                    NewQuantity = 0,
                    PriceType = PriceType.Default
                }, metadata);
            }

            await ConnectionTester(AmendOrdersFunc);
            return response;
        }

        internal async Task<DeleteOrderResponse> DeleteOrder(string id, Metadata metadata)
        {
            DeleteOrderResponse response = null;

            async Task DeleteOrderFunc()
            {
                response = await _client.DeleteOrderAsync(new DeleteOrderRequest
                {
                    OrderId = id
                }, metadata);
                Log.Information("Deleting was {0} {1}", response.Response.Code, response.Response.Message);
            }

            await ConnectionTester(DeleteOrderFunc);
            return response;
        }

        internal void StartObserving(Metadata meta)
        {
            _token = new CancellationTokenSource();
            _ = ObservePositions(meta);
            _ = ObserveBalance(meta);
            _ = ObserveMarketPrices(meta);
            _ = ObserveMyOrders(meta);
        }

        internal void StopObserving()
        {
            _token.Cancel();
        }

    }
}
