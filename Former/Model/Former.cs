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
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            var price = type == OrderType.Buy
                ? oldOrder.Price - oldOrder.Price * _configuration.RequiredProfit
                : oldOrder.Price + oldOrder.Price * _configuration.RequiredProfit;
            
            var addResponse = false;
            Order newOrder = null;
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, -quantity, _metadata);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                newOrder = new Order
                {
                    Id = placeResponse.OrderId,
                    Price = price,
                    //Поставил минус
                    Quantity = -quantity,
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
                await _historyClient.WriteOrder(newOrder, ChangesType.Insert, _metadata, "Counter order placed");
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

            if (_storage.MyOrders.Count(x => x.Value.Signature.Type == OrderType.Buy) > 0 ||
                _storage.CounterOrders.Count(x => x.Value.Signature.Type == OrderType.Buy) > 0 &&
                type == OrderType.Sell)
                return false;
            if (_storage.MyOrders.Count(x => x.Value.Signature.Type == OrderType.Sell) > 0 ||
                _storage.CounterOrders.Count(x => x.Value.Signature.Type == OrderType.Sell) > 0 &&
                type == OrderType.Buy)
                return false;
            var orderCost = _configuration.ContractValue / (type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice);
            var totalBalance = ConvertSatoshiToXBT(_storage.TotalBalance);
            var availableBalance = ConvertSatoshiToXBT(_storage.AvailableBalance);
            if (-totalBalance * (_configuration.AvaibleBalance - 1) + availableBalance > orderCost) return true;
            Log.Debug("{@Where}: Cannot place {@Type} order. Insufficient balance.", "Former", type);
            return false;
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
                _storage.MyOrders.TryRemove(key, out _);
                var response = await _tradeMarketClient.DeleteOrder(key, _metadata);
                if (response.Response.Code != ReplyCode.Succeed) return;
                Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", value.Id, value.Price, value.Quantity, value.Signature.Type, response.Response.Code == ReplyCode.Succeed ? ReplyCode.Succeed : ReplyCode.Failure);
                await _historyClient.WriteOrder(value, ChangesType.Delete, _metadata, "Removed by user");
            }
        }

    }
}
