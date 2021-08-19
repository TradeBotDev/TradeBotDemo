using System;
using System.Threading;
using System.Threading.Tasks;
using Former.Models;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using TradeBot.TradeMarket.TradeMarketService.v1;
using Metadata = Grpc.Core.Metadata;

namespace Former.Clients
{
    public class TradeMarketClient
    {
        internal delegate Task MyOrdersEvent(Order newComingOrder, ChangesType changesType);
        internal MyOrdersEvent UpdateMyOrders;

        internal delegate Task BalanceEvent(int availableBalance, int totalBalance);
        internal BalanceEvent UpdateBalance;

        internal delegate Task PositionEvent(double positionQuantity);
        internal PositionEvent UpdatePosition;

        internal delegate Task MarketPricesEvent(double bid, double ask);
        internal MarketPricesEvent UpdateMarketPrices;

        internal delegate Task LotSizeEvent(int lotSize);
        internal LotSizeEvent UpdateLotSize;

        private static int _retryDelay;
        private static string _connectionString;
        private CancellationTokenSource _token;

        private readonly TradeMarketService.TradeMarketServiceClient _client;

        internal static void Configure(string connectionString, int retryDelay)
        {
            _connectionString = connectionString;
            _retryDelay = retryDelay;
        }

        internal TradeMarketClient()
        {
            _client = new TradeMarketService.TradeMarketServiceClient(GrpcChannel.ForAddress(_connectionString));
        }

        /// <summary>
        /// Проверяет соединение с биржей, на вход принимает функцию, осуществляющую общение с биржей.
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

        private bool EventFilter(Metadata incomingMeta, Metadata filteringMeta)
        {
            bool IsEqual(Metadata incomingMeta, Metadata filteringMeta, string key)
            {
                return (incomingMeta.GetValue(key) is not null &&
                        incomingMeta.GetValue(key) == filteringMeta.GetValue(key)) 
                       || incomingMeta.GetValue(key) is null;
            }


            return IsEqual(incomingMeta,filteringMeta,"sessionid") && IsEqual(incomingMeta,filteringMeta,"slot") && IsEqual(incomingMeta,filteringMeta,"trademarket");
            //return filteringMeta[0] == incomingMeta[0] && filteringMeta[1] == incomingMeta[1] && filteringMeta[2] == incomingMeta[2];
        }

        /// <summary>
        /// Наблюдает за изменением рыночных цен.
        /// </summary>
        private async Task ObserveMarketPrices(Metadata meta)
        {
            using var call = _client.SubscribePrice(new SubscribePriceRequest(), meta);
            async Task ObserveMarketPricesFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    if (EventFilter(call.ResponseHeadersAsync.Result, meta))
                    {
                        await UpdateMarketPrices?.Invoke(call.ResponseStream.Current.BidPrice, call.ResponseStream.Current.AskPrice);
                        await UpdateLotSize?.Invoke(call.ResponseStream.Current.LotSize);
                    }
                }
            }

            await ConnectionTester(ObserveMarketPricesFunc);
        }

        /// <summary>
        /// Наблюдает за обновлением доступного баланса.
        /// </summary>
        private async Task ObserveBalance(Metadata meta)
        {
            using var call = _client.SubscribeMargin(new SubscribeMarginRequest(), meta);

            async Task ObserveBalanceFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    if (EventFilter(call.ResponseHeadersAsync.Result, meta)) await UpdateBalance?.Invoke((int) call.ResponseStream.Current.Margin.AvailableMargin, (int)call.ResponseStream.Current.Margin.MarginBalance);
                }
            }
            await ConnectionTester(ObserveBalanceFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих ордеров.
        /// </summary>
        private async Task ObserveMyOrders(Metadata meta)
        {
            using var call = _client.SubscribeMyOrders(new SubscribeMyOrdersRequest(), meta);

            async Task ObserveMyOrdersFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    if (EventFilter(call.ResponseHeadersAsync.Result, meta)) await UpdateMyOrders?.Invoke(Converters.ConvertOrder(call.ResponseStream.Current.Changed), (ChangesType)call.ResponseStream.Current.ChangesType);
                }
            }

            await ConnectionTester(ObserveMyOrdersFunc);
        }

        /// <summary>
        /// Наблюдает за событиями моих позиций.
        /// </summary>
        private async Task ObservePositions(Metadata meta)
        {
            using var call = _client.SubscribePosition(new SubscribePositionRequest(), meta);

            async Task ObservePositionFunc()
            {
                while (await call.ResponseStream.MoveNext(_token.Token))
                {
                    if (EventFilter(call.ResponseHeadersAsync.Result, meta)) await UpdatePosition?.Invoke(call.ResponseStream.Current.CurrentQty);
                }
            }

            await ConnectionTester(ObservePositionFunc);
        }

        /// <summary>
        /// Отправляет запрос в биржу на выставление своего ордера.
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
        /// Отправляет запрос в биржу на изменение цены своего ордера.
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

        /// <summary>
        /// Отправляет запрос на биржу на удаление своего ордера по id.
        /// </summary>
        internal async Task<DeleteOrderResponse> DeleteOrder(string id, Metadata metadata)
        {
            DeleteOrderResponse response = null;

            async Task DeleteOrderFunc()
            {
                response = await _client.DeleteOrderAsync(new DeleteOrderRequest
                {
                    OrderId = id
                }, metadata);
            }

            await ConnectionTester(DeleteOrderFunc);
            return response;
        }


        /// <summary>
        /// Подписывается на обновления с биржи по указанным метаданным, а также создаёт CancellationTokenSource, для дальнейшей отмены.
        /// </summary>
        internal void StartObserving(Metadata meta)
        {
            _token = new CancellationTokenSource();
            _ = ObservePositions(meta);
            _ = ObserveBalance(meta);
            _ = ObserveMarketPrices(meta);
            _ = ObserveMyOrders(meta);
        }

        /// <summary>
        /// Отменяет подписку на обновления с биржи путём отмены токена, созданного ранее.
        /// </summary>
        internal void StopObserving()
        {
            _token.Cancel();
        }
    }
}
