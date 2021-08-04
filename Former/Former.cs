using Google.Protobuf.WellKnownTypes;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Former
    {
        private readonly Storage _storage;

        public Former(Storage storage)
        {
            _storage = storage;
            _storage.PlaceOrderEvent += PlaceCounterOrder;
        }

        /// <summary>
        /// Выставляет контр-ордер на основе информации старого и обновлённого ордера, и добавляет в список контр-ордеров
        /// </summary>
        public async Task PlaceCounterOrder(Order oldOrder, Order newComingOrder, UserContext context)
        {
            var quantity = oldOrder.Quantity - newComingOrder.Quantity;
            var price = oldOrder.Signature.Type == OrderType.Buy ? oldOrder.Price + oldOrder.Price * context.Configuration.RequiredProfit : oldOrder.Price - oldOrder.Price * context.Configuration.RequiredProfit;
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            var addResponse = false;

            var placeResponse = await context.PlaceOrder(price, -quantity);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                addResponse = _storage.AddOrder(placeResponse.OrderId,
                    new Order
                    {
                        Id = placeResponse.OrderId,
                        Price = price,
                        Quantity = -quantity,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = type },
                        LastUpdateDate = new Timestamp()
                    }, _storage.CounterOrders);
            }
            Log.Information("Counter order {0} price: {1}, quantity: {2} placed {3} {4}", oldOrder.Id, price, -quantity, placeResponse.Response.Code, placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            Log.Information("Order {0}, price: {1}, quantity: {2}, type: {3} added to counter orders list {4}", placeResponse.OrderId, price, -quantity, type, addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type, UserContext context)
        {
            var orderCost = context.Configuration.ContractValue / (type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice);
            var availableBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_storage.AvailableBalance) * context.Configuration.AvaibleBalance;
            var totalBalanceWithConfigurationReduction = ConvertSatoshiToXBT(_storage.TotalBalance) * context.Configuration.AvaibleBalance;

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
            if (_storage.PlaceLocker) return;
            var orderType = decision > 0 ? OrderType.Buy : OrderType.Sell;
            if (!CheckPossibilityPlacingOrder(orderType, context)) return;

            var quantity = orderType == OrderType.Buy ? context.Configuration.ContractValue : -context.Configuration.ContractValue;
            var price = orderType == OrderType.Buy ? _storage.BuyMarketPrice : _storage.SellMarketPrice;

            var response = await context.PlaceOrder(price, quantity);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                var addResponse = _storage.AddOrder(response.OrderId,
                    new Order
                    {
                        Id = response.OrderId,
                        Price = price,
                        Quantity = quantity,
                        Signature = new OrderSignature { Status = OrderStatus.Open, Type = orderType },
                        LastUpdateDate = new Timestamp()
                    }, _storage.MyOrders);
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
    }
}
