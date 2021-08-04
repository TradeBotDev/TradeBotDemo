using System;
using System.Threading.Tasks;
using Serilog;
using TradeBot.Common.v1;

namespace Former
{
    public class UpdateHandlers
    {
        private bool _fitPricesLocker;
        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        public async Task CheckAndFitPrices(UserContext context)
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if (Math.Abs(_sellMarketPrice - _savedMarketSellPrice) > 0.4 || Math.Abs(_savedMarketBuyPrice - _buyMarketPrice) > 0.4)
            {
                _savedMarketSellPrice = _sellMarketPrice;
                _savedMarketBuyPrice = _buyMarketPrice;
                Log.Information("Buy market price: {0}, Sell market price: {1}", _buyMarketPrice, _sellMarketPrice);
                if (!_fitPricesLocker && !_myOrders.IsEmpty) await FitPrices(context);
            }
        }

        /// <summary>
        /// Обновляет цену ордера из _myOrders
        /// </summary>
        public void UpdateOrderPrice(Order order, double price)
        {
            _myOrders.AddOrUpdate(order.Id, order, (_, v) =>
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
            return type == OrderType.Sell ? _sellMarketPrice : _buyMarketPrice;
        }

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        public async Task FitPrices(UserContext context)
        {
            if (CheckContext(context)) return;
            _fitPricesLocker = true;

            var ordersSuitableForUpdate = _myOrders.Where(pair => Math.Abs(pair.Value.Price - GetFairPrice(pair.Value.Signature.Type)) >= context.Configuration.OrderUpdatePriceRange);
            foreach (var (key, order) in ordersSuitableForUpdate)
            {
                var fairPrice = GetFairPrice(order.Signature.Type);
                var response = await context.AmendOrder(order.Id, fairPrice);

                if (response.Response.Code == ReplyCode.Succeed) UpdateOrderPrice(order, fairPrice);
                else if (response.Response.Message.Contains("Invalid ordStatus"))
                {
                    var removeResponse = RemoveOrder(key, _myOrders);
                    Log.Information("My order {0}, price: {1}, quantity: {2}, type: {3} removed cause cannot be amended {4} ", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                }
                Log.Information("Order {0} amended with {1} {2} {3}", key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Succeed ? "" : response.Response.Message);
            }
            _fitPricesLocker = false;
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
