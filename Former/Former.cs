using Google.Protobuf.WellKnownTypes;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.TradeMarket.TradeMarketService.v1;

namespace Former
{
    public class Former
    {


        /// <summary>
        /// Выставляет контр-ордер на основе информации старого и обновлённого ордера, и добавляет в список контр-ордеров
        /// </summary>
        private async Task PlaceCounterOrder(Order oldOrder, Order newComingOrder, UserContext context)
        {
            var quantity = oldOrder.Quantity - newComingOrder.Quantity;
            var price = oldOrder.Signature.Type == OrderType.Buy ? oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit : oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            var addResponse = false;

            var placeResponse = await context.PlaceOrder(price, -quantity);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                addResponse = AddOrder(placeResponse.OrderId,
                    new Order
                    {
                        Id = placeResponse.OrderId,
                        Price = price,
                        Quantity = -quantity,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = type },
                        LastUpdateDate = new Timestamp()
                    }, _counterOrders);
            }
            Log.Information("Counter order {0} price: {1}, quantity: {2} placed {3} {4}", oldOrder.Id, price, -quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3} added to counter orders list {4}", placeResponse.OrderId, price, -quantity, type, addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type, UserContext context)
        {
            var orderCost = context.Configuration.ContractValue / (type == OrderType.Sell ? _sellMarketPrice : _buyMarketPrice);
            var availableBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_totalBalance) * context.Configuration.AvaibleBalance;

            double marginOfAlreadyPlacedSellOrders = 0;
            double marginOfAlreadyPlacedBuyOrders = 0;

            if (!_myOrders.IsEmpty || !_counterOrders.IsEmpty)
            {
                var marginOfMySellOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterSellOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Sell).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedSellOrders = marginOfMySellOrders + marginOfCounterSellOrders;

                var marginOfMyBuyOrders = _myOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                var marginOfCounterBuyOrders = _counterOrders.Where(x => x.Value.Signature.Type == OrderType.Buy).Sum(x => x.Value.Quantity / x.Value.Price);
                marginOfAlreadyPlacedBuyOrders = marginOfMyBuyOrders + marginOfCounterBuyOrders;
            }

            switch (type == OrderType.Sell ? -_positionSize : _positionSize)
            {
                case > 0 when availableBalanceWithConfigurationReduction < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient available balance.", type);
                    return false;
                case <= 0 when totalBalanceWithConfigurationReduction + marginOfAlreadyPlacedSellOrders - marginOfAlreadyPlacedBuyOrders < orderCost:
                    Log.Debug("Cannot place {0} order. Insufficient total balance.", type);
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Формирует в зависимости от решения алгоритма
        /// </summary>
        public async Task FormOrder(int decision, UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            var orderType = decision > 0 ? OrderType.Buy : OrderType.Sell;
            if (!CheckPossibilityPlacingOrder(orderType, context)) return;

            var quantity = orderType == OrderType.Buy ? context.Configuration.ContractValue : -context.Configuration.ContractValue;
            var price = orderType == OrderType.Buy ? _buyMarketPrice : _sellMarketPrice;

            var response = await context.PlaceOrder(price, quantity);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                var addResponse = AddOrder(response.OrderId,
                    new Order
                    {
                        Id = response.OrderId,
                        Price = price,
                        Quantity = quantity,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = orderType },
                        LastUpdateDate = new Timestamp()
                    }, _myOrders);
                Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3} added to my orders list {4}", response.OrderId, price, quantity, orderType, addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
            }
            Log.Information("Order {0} price: {1}, quantity: {2} placed for {3} {4} {5}", response.OrderId, price, quantity, orderType, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        private double ConvertSatoshiToXBT(int satoshiValue)
        {
            return satoshiValue * 0.00000001;
        }

        /// <summary>
        /// Возвращает true если контекст нулевой или имеет нулевые поля, false иначе
        /// </summary>
        private bool CheckContext(UserContext context)
        {
            if (context is null)
            {
                Log.Error("Bad user context (null)");
                return true;
            }
            if (context.Configuration is null || context.SessionId is null || context.Slot is null || context.TradeMarket is null)
            {
                Log.Error("Bad user context (some field is null)");
                return true;
            }
            return false;
        }
    }
}
