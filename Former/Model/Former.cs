using System;
using Former.Clients;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former.Model
{
    public class Former
    {
        private readonly Storage _storage;
        private Config _configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Metadata _metadata;
        private readonly HistoryClient _historyClient;

        internal Former(Storage storage, Config configuration, TradeMarketClient tradeMarketClient, Metadata metadata, HistoryClient historyClient)
        {
            _storage = storage;
            _storage.PlaceOrderEvent += PlaceCounterOrder;
            _configuration = configuration;
            _tradeMarketClient = tradeMarketClient;
            _metadata = metadata;
            _historyClient = historyClient;
        }

        internal void SetConfiguration(Config configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Выставляет контр-ордер на основе информации старого и обновлённого ордера, и добавляет в список контр-ордеров
        /// </summary>
        private async Task PlaceCounterOrder(Order oldOrder, Order newComingOrder)
        {
            var quantity = oldOrder.Quantity - newComingOrder.Quantity;
            var price = oldOrder.Signature.Type == OrderType.Buy
                ? oldOrder.Price + oldOrder.Price * _configuration.RequiredProfit
                : oldOrder.Price - oldOrder.Price * _configuration.RequiredProfit;
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            var addResponse = false;
            Order newOrder = null;
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, -quantity, _metadata);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                newOrder = new Order
                {
                    Id = placeResponse.OrderId,
                    Price = price,
                    Quantity = quantity,
                    Signature = new OrderSignature { Status = OrderStatus.Open, Type = type },
                    LastUpdateDate = new Timestamp()
                };
                addResponse = _storage.AddOrder(placeResponse.OrderId, newOrder, _storage.CounterOrders);
            }

            Log.Information(
                "{@Where}: Counter order {@Id} price: {@Price}, quantity: {@Quantity} placed {@ResponseCode} {@ResponseMessage}",
                "Former", oldOrder.Id, price, -quantity, placeResponse.Response.Code,
                placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            Log.Information(
                "{@Where}: Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@ResponseCode} added to counter orders list {@ResponseMessage}",
                "Former", placeResponse.OrderId, price, -quantity, type,
                addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
            if (Convert.ToInt32(quantity) == Convert.ToInt32(oldOrder.Quantity))
            {
                await _historyClient.WriteOrder(oldOrder, ChangesType.Delete, _metadata, "Initial order filled");
                await _historyClient.WriteOrder(oldOrder, ChangesType.Insert, _metadata, "Counter order placed");
            }
            else
            {
                await _historyClient.WriteOrder(newComingOrder, ChangesType.Update, _metadata, "Initial order partially filled");
                await _historyClient.WriteOrder(newOrder, ChangesType.Insert, _metadata, "Counter order placed");
            }
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type)
        {
            var orderCost = _configuration.ContractValue / (type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice);
            var availableBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_storage.AvailableBalance) * _configuration.AvaibleBalance;
            var totalBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_storage.TotalBalance) * _configuration.AvaibleBalance;

            double marginOfAlreadyPlacedSellOrders = 0;
            double marginOfAlreadyPlacedBuyOrders = 0;

            if (!_storage.MyOrders.IsEmpty || !_storage.CounterOrders.IsEmpty)
            {
                var marginOfMySellOrders = _storage.MyOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterSellOrders = _storage.CounterOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedSellOrders = marginOfMySellOrders + marginOfCounterSellOrders;

                var marginOfMyBuyOrders = _storage.MyOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterBuyOrders = _storage.CounterOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedBuyOrders = marginOfMyBuyOrders + marginOfCounterBuyOrders;
            }

            switch (type == OrderType.Sell ? -_storage.PositionSize : _storage.PositionSize)
            {
                case > 0 when availableBalanceWithConfigurationReduction < orderCost:
                    Log.Debug("{@Where}: Cannot place {@Type} order. Insufficient available balance.", "Former", type);
                    return false;
                case <= 0 when totalBalanceWithConfigurationReduction + marginOfAlreadyPlacedSellOrders - marginOfAlreadyPlacedBuyOrders < orderCost:
                    Log.Debug("{@Where}: Cannot place {@Type} order. Insufficient total balance.", "Former", type);
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Формирует в зависимости от решения алгоритма
        /// </summary>
        internal async Task FormOrder(int decision)
        {
            if (_storage.PlaceLocker) return;
            var orderType = decision > 0 ? OrderType.Buy : OrderType.Sell;
            if (!CheckPossibilityPlacingOrder(orderType)) return;

            Order newOrder = null;
            var quantity = orderType == OrderType.Buy ? _configuration.ContractValue : -_configuration.ContractValue;
            var price = orderType == OrderType.Buy ? _storage.BuyMarketPrice : _storage.SellMarketPrice;

            var response = await _tradeMarketClient.PlaceOrder(price, quantity, _metadata);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                newOrder = new Order
                {
                    Id = response.OrderId,
                    Price = price,
                    Quantity = quantity,
                    Signature = new OrderSignature { Status = OrderStatus.Open, Type = orderType },
                    LastUpdateDate = new Timestamp()
                };
                var addResponse = _storage.AddOrder(response.OrderId, newOrder, _storage.MyOrders);
                Log.Information(
                    "{@Where}: Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} added to my orders list {@ResponseCode}",
                    "Former", response.OrderId, price, quantity, orderType,
                    addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
            }

            Log.Information(
                "{@Where}: Order {@Id} price: {@Price}, quantity: {@Quantity} placed for {@Type} {@ResponseCode} {@ResponseMessage}",
                "Former", response.OrderId, price, quantity, orderType, response.Response.Code.ToString(),
                response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
            await _historyClient.WriteOrder(newOrder, ChangesType.Insert, _metadata, "Initial order placed");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        private double ConvertSatoshiToXBT(int satoshiValue)
        {
            return satoshiValue * 0.00000001;
        }

        internal async Task RemoveAllMyOrders()
        {
            foreach (var (key, value) in _storage.MyOrders)
            {
                var response = await _tradeMarketClient.DeleteOrder(key, _metadata);
                Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", value.Id, value.Price, value.Quantity, value.Signature.Type, response.Response.Code == ReplyCode.Succeed ? ReplyCode.Succeed : ReplyCode.Failure);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _storage.MyOrders.TryRemove(key, out _);
                    await _historyClient.WriteOrder(value, ChangesType.Delete, _metadata, "Removed by user");
                }
                else return;
            }
        }

    }
}
