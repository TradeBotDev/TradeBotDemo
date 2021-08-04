using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using TradeBot.Common.v1;

namespace Former
{
    public class UpdateHandlers
    {
        private readonly Storage _storage;

        public UpdateHandlers(Storage storage)
        {
            _storage = storage;
            _storage.HandleUpdateEvent += CheckAndFitPrices;
        }

        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        public async Task CheckAndFitPrices(UserContext context)
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if (Math.Abs(_storage.SellMarketPrice - _storage.SavedMarketSellPrice) > 0.4 || Math.Abs(_storage.SavedMarketBuyPrice - _storage.BuyMarketPrice) > 0.4)
            {
                _storage.SavedMarketSellPrice = _storage.SellMarketPrice;
                _storage.SavedMarketBuyPrice = _storage.BuyMarketPrice;
                Log.Information("Buy market price: {0}, Sell market price: {1}", _storage.BuyMarketPrice, _storage.SellMarketPrice);
                if (!_storage.FitPricesLocker && ! _storage.MyOrders.IsEmpty) await FitPrices(context);
            }
        }

        /// <summary>
        /// Обновляет цену ордера из MyOrders
        /// </summary>
        public void UpdateOrderPrice(Order order, double price)
        {
            _storage.MyOrders.AddOrUpdate(order.Id, order, (_, v) =>
            {
                v.Price = price;
                v.Quantity = v.Quantity;
                v.LastUpdateDate = v.LastUpdateDate;
                v.Signature = v.Signature;
                return v;
            });
        }

        public double GetFairPrice(OrderType type)
        {
            return type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice;
        }

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        public async Task FitPrices(UserContext context)
        {
            _storage.FitPricesLocker = true;

            var ordersSuitableForUpdate = _storage.MyOrders.Where(pair => Math.Abs(pair.Value.Price - GetFairPrice(pair.Value.Signature.Type)) >= context.Configuration.OrderUpdatePriceRange);
            foreach (var (key, order) in ordersSuitableForUpdate)
            {
                var fairPrice = GetFairPrice(order.Signature.Type);
                var response = await context.AmendOrder(order.Id, fairPrice);

                if (response.Response.Code == ReplyCode.Succeed) UpdateOrderPrice(order, fairPrice);
                else if (response.Response.Message.Contains("Invalid ordStatus"))
                {
                    var removeResponse = _storage.RemoveOrder(key, _storage.MyOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed cause cannot be amended {4} ", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                }
                Log.Information("Order {0} amended with {1} {2} {3}", key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Succeed ? "" : response.Response.Message);
            }
            _storage.FitPricesLocker = false;
        }
    }
}
