using System;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
using Grpc.Core;
using Serilog;

namespace Former.Model
{
    public class UpdateHandlers
    {
        private readonly Storage _storage;
        private Configuration _configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Metadata _metadata;
        private readonly HistoryClient _historyClient; 

        private double _oldMarketBuyPrice;
        private double _oldMarketSellPrice;
        private int _oldTotalBalance;


        internal UpdateHandlers(Storage storage, Configuration configuration, TradeMarketClient tradeMarketClient, Metadata metadata, HistoryClient historyClient)
        {
            _storage = storage;
            _storage.HandleUpdateEvent += MainUpdateHandler;
            _configuration = configuration;
            _metadata = metadata;
            _tradeMarketClient = tradeMarketClient;
            _historyClient = historyClient;
        }

        /// <summary>
        /// Обвновляет конфигурацию в UpdateHandlers (позволяет изменять конфигурацию во время работы)
        /// </summary>
        internal void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Проверяет, стоит ли перевыставлять ордера, и вызывает FitPrices, в том случае, если это необходимо
        /// </summary>
        private async Task MainUpdateHandler(Order order, ChangesType changesType)
        {
            if (Math.Abs(_storage.SellMarketPrice - _oldMarketSellPrice) > 0.4 || Math.Abs(_oldMarketBuyPrice - _storage.BuyMarketPrice) > 0.4)
            {
                //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах и подогнать их к рыночной цене 
                //с помощью метода FitPrices
                _oldMarketSellPrice = _storage.SellMarketPrice;
                _oldMarketBuyPrice = _storage.BuyMarketPrice;
                //Log.Information("{@Where}: Buy market price: {@BuyMarketPrice}, Sell market price: {@SellMarketPrice}", "Former",_storage.BuyMarketPrice, _storage.SellMarketPrice);
                if (!_storage.FitPricesLocker && !_storage.MyOrders.IsEmpty) await FitPrices();
            }
            if (_oldTotalBalance != _storage.TotalBalance)
            {
                //если баланс изменился, необходимо отправить новый баланс в историю
                _oldTotalBalance= _storage.TotalBalance;
                await _historyClient.WriteBalance(_storage.TotalBalance, _metadata);
            }
            if (order is not null)
            {
                //здесь сообщается истории об инициализации оредра или о его удалении (это касается только контр-ордеров)
                if (changesType == ChangesType.CHANGES_TYPE_PARTITIAL) await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_PARTITIAL, _metadata, "Counter order initialized");
                if (changesType == ChangesType.CHANGES_TYPE_DELETE) await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_DELETE, _metadata, "Counter order filled");
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
            return type == OrderType.ORDER_TYPE_SELL ? _storage.SellMarketPrice : _storage.BuyMarketPrice;
        }

        /// <summary>
        /// Подгоняет мои ордера под рыночную цену
        /// </summary>
        private async Task FitPrices()
        {
            _storage.FitPricesLocker = true;
            //выбираем ордера из списка своих ордеров, которые необходимо подогнать к новой рыночной цене
            var ordersSuitableForUpdate = _storage.MyOrders.Where(pair => Math.Abs(pair.Value.Price - GetFairPrice(pair.Value.Signature.Type)) >= _configuration.OrderUpdatePriceRange);
            foreach (var (key, order) in ordersSuitableForUpdate)
            {
                var fairPrice = GetFairPrice(order.Signature.Type);
                //отправляем запрос на изменение цены ордера по его id
                var ammendOrderResponse = await _tradeMarketClient.AmendOrder(order.Id, fairPrice, _metadata);
                var response  = Converters.ConvertDefaultResponse(ammendOrderResponse.Response);

                if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
                {
                    //в случае положительного ответа обновляем его в своём списке и сообщаем об изменениях истории
                    UpdateOrderPrice(order, fairPrice);
                    await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_UPDATE, _metadata, "Order amended");
                }
                else if (response.Message.Contains("Invalid ordStatus"))
                {
                    //при получении ошибки Invalid ordStatus мы понимаем, что пытаемся изменить ордер, которого нет на бирже, 
                    //но при этом он есть у нас в списках, поэтому мы удаляем его из своих списков и сообщаем об удалении истории
                    await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_DELETE, _metadata, "");
                    var removeResponse = _storage.RemoveOrder(key, _storage.MyOrders);
                    Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed cause cannot be amended {@ResponseCode} ", "Former", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                } else return;
                Log.Information("{@Where}: Order {@Id} amended with {@Price} {@ResponseCode} {@ResponseMessage}", "Former", key, fairPrice, response.Code.ToString(), response.Code == ReplyCode.REPLY_CODE_SUCCEED ? "" : response.Message);
            }
            _storage.FitPricesLocker = false;
        }
    }
}
