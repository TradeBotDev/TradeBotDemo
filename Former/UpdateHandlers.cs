using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Serilog;
using TradeBot.Common.v1;

namespace Former
{
    public class UpdateHandlers
    {
        private readonly Storage _storage;
        private readonly Config _configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Metadata _metadata;
        private readonly Logger _logger; 

        internal UpdateHandlers(Storage storage, Config configuration, TradeMarketClient tradeMarketClient, Metadata metadata, Logger logger)
        {
            _storage = storage;
            _storage.HandleUpdateEvent += CheckAndFitPrices;
            _configuration = configuration;
            _metadata = metadata;
            _tradeMarketClient = tradeMarketClient;
            _logger = logger;
        }

        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        private async Task CheckAndFitPrices()
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах
            if (Math.Abs(_storage.SellMarketPrice - _storage.SavedMarketSellPrice) > 0.4 || Math.Abs(_storage.SavedMarketBuyPrice - _storage.BuyMarketPrice) > 0.4)
            {
                _storage.SavedMarketSellPrice = _storage.SellMarketPrice;
                _storage.SavedMarketBuyPrice = _storage.BuyMarketPrice;
                Log.Information("{@Where}: Buy market price: {@BuyMarketPrice}, Sell market price: {@SellMarketPrice}", "Former",_storage.BuyMarketPrice, _storage.SellMarketPrice);
                if (!_storage.FitPricesLocker && ! _storage.MyOrders.IsEmpty) await FitPrices();
            }
        }

        /// <summary>
        /// Обновляет цену ордера из MyOrders
        /// </summary>
        private void UpdateOrderPrice(Order order, double price)
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

        /// <summary>
        /// Получить рыночную цену по типу ордера
        /// </summary>
        private double GetFairPrice(OrderType type)
        {
            return type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice;
        }

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices()
        {
            _storage.FitPricesLocker = true;

            var ordersSuitableForUpdate = _storage.MyOrders.Where(pair => Math.Abs(pair.Value.Price - GetFairPrice(pair.Value.Signature.Type)) >= _configuration.OrderUpdatePriceRange);
            foreach (var (key, order) in ordersSuitableForUpdate)
            {
                var fairPrice = GetFairPrice(order.Signature.Type);
                var response = await _tradeMarketClient.AmendOrder(order.Id, fairPrice, _metadata);

                if (response.Response.Code == ReplyCode.Succeed) UpdateOrderPrice(order, fairPrice);
                else if (response.Response.Message.Contains("Invalid ordStatus"))
                {
                    var removeResponse = _storage.RemoveOrder(key, _storage.MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed cause cannot be amended {@ResponseCode} ", "Former", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.Succeed : ReplyCode.Failure);
                }
                Log.Information("{@Where}: Order {@Id} amended with {@Price} {@ResponseCode} {@ResponseMessage}", "Former", key, fairPrice, response.Response.Code.ToString(), response.Response.Code == ReplyCode.Succeed ? "" : response.Response.Message);
            }
            _storage.FitPricesLocker = false;
        }
    }
}
