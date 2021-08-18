using System;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
using Grpc.Core;
using Serilog;

namespace Former.Models
{
    public class Former
    {
        private readonly Storage _storage;
        private Configuration _configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Metadata _metadata;
        private readonly HistoryClient _historyClient;

        internal Former(Storage storage, Configuration configuration, TradeMarketClient tradeMarketClient, Metadata metadata, HistoryClient historyClient)
        {
            _storage = storage;
            _storage.PlaceOrderEvent += PlaceCounterOrder;
            _configuration = configuration;
            _tradeMarketClient = tradeMarketClient;
            _metadata = metadata;
            _historyClient = historyClient;
        }
        /// <summary>
        /// Обвновляет конфигурацию в Former (позволяет изменять конфигурацию во время работы)
        /// </summary>
        internal void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Выставляет контр-ордер на основе информации старого и обновлённого ордера, и добавляет в список контр-ордеров
        /// </summary>
        private async Task PlaceCounterOrder(Order oldOrder, Order newComingOrder)
        {
            //число контрактов контр ордера (работает для полного и для частичного контр-ордера)
            var quantity = oldOrder.Quantity - newComingOrder.Quantity;
            //тип ордера с которым необходимо выставить контр-ордер
            var type = oldOrder.Signature.Type == OrderType.ORDER_TYPE_BUY ? OrderType.ORDER_TYPE_SELL : OrderType.ORDER_TYPE_BUY;
            //цена контр ордера, которая зависит цены старого ордера и типа выставляемого ордера с учётом необходимого профита
            var price = type == OrderType.ORDER_TYPE_BUY
                ? oldOrder.Price - oldOrder.Price * _configuration.RequiredProfit
                : oldOrder.Price + oldOrder.Price * _configuration.RequiredProfit;

            //ответ при добавлении контр-ордера в соответсвующий список (необходимо для логов)
            var addResponse = false;
            //выставляем контр ордер с рассчитанной ценой и отрицательным числом контрактов, так как контр ордер должен иметь противоположное число 
            //контрактов по сравнению с oldOrder
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, -quantity, _metadata);
            var response = Converters.ConvertDefaultResponse(placeResponse.Response);
            if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
            {
                //в случае положительного ответа от биржи вносим контр-ордер в соответсвующий список с полученным в ответе OrderId
                var newOrder = new Order
                {
                    Id = placeResponse.OrderId,
                    Price = price,
                    //Поставил минус
                    Quantity = -quantity,
                    Signature = new OrderSignature { Status = OrderStatus.ORDER_STATUS_OPEN, Type = type },
                    LastUpdateDate = new DateTime()
                };
                addResponse = _storage.AddOrder(placeResponse.OrderId, newOrder, _storage.CounterOrders);

                //сообщаем о выставлении контр-ордера истории
                await _historyClient.WriteOrder(newOrder, ChangesType.CHANGES_TYPE_INSERT, _metadata, "Counter order placed");

                //сообщаем об исполнении старого ордера истории
                if (Convert.ToInt32(quantity) == Convert.ToInt32(oldOrder.Quantity))
                    await _historyClient.WriteOrder(oldOrder, ChangesType.CHANGES_TYPE_DELETE, _metadata, "Initial order filled");
                else
                    await _historyClient.WriteOrder(newComingOrder, ChangesType.CHANGES_TYPE_UPDATE, _metadata,
                        "Initial order partially filled");
            }

            Log.Information(
                "{@Where}: Counter order {@Id} price: {@Price}, quantity: {@Quantity} placed {@ResponseCode} {@ResponseMessage}",
                "Former", oldOrder.Id, price, -quantity, placeResponse.Response.Code,
                response.Code == ReplyCode.REPLY_CODE_SUCCEED ? "" : placeResponse.Response.Message);
            Log.Information(
                "{@Where}: Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@ResponseCode} added to counter orders list {@ResponseMessage}",
                "Former", placeResponse.OrderId, price, -quantity, type,
                addResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type)
        {
            
            //вычисляем предполагаемую стоимость ордера по рыночной цене в биткоинах
            var orderCost = _configuration.ContractValue / (type == OrderType.ORDER_TYPE_SELL ? _storage.SellMarketPrice : _storage.BuyMarketPrice);
            //конвертируем баланс в биткоины (XBT), так как он приходит от биржи в сатоши (XBt)
            var totalBalance = ConvertSatoshiToXBT(_storage.TotalBalance);
            var availableBalance = ConvertSatoshiToXBT(_storage.AvailableBalance);
            //проверяем, возможно ли выставить с текущим общим и доступным балансом с учётом настройки
            if (totalBalance * (_configuration.AvailableBalance - 1) + availableBalance > orderCost) return true;
            Log.Debug("{@Where}: Cannot place {@Type} order. Insufficient balance.", "Former", type);
            return false;
        }

        /// <summary>
        /// Если у нас уже есть ордера на этого типа, запрещаем ставить ордер противоположного типа во избежание неверного подсчёта доступного баланса.
        /// </summary>
        private bool CheckPosition(OrderType type)
        {
            //проверяет, есть ли уже ордера противоположного типа и возвращает false, если таковые имеются.
            var alreadyPresentOrderTypes = type == OrderType.ORDER_TYPE_SELL ? OrderType.ORDER_TYPE_BUY : OrderType.ORDER_TYPE_SELL;
            return _storage.MyOrders.All(x => x.Value.Signature.Type != alreadyPresentOrderTypes) && _storage.CounterOrders.All(x => x.Value.Signature.Type != alreadyPresentOrderTypes);
        }

        /// <summary>
        /// Формирует в зависимости от решения алгоритма
        /// </summary>
        internal async Task FormOrder(int decision)
        {
            if (_storage.PlaceLocker) return;
            //тип выставляемого ордера в зависимости от решения алгоритма
            var orderType = decision > 0 ? OrderType.ORDER_TYPE_BUY : OrderType.ORDER_TYPE_SELL;

            //проверяем можно ли поставить ордер такого типа в текущей позиции
            if (!CheckPosition(orderType)) return;
            //проверяем возможность выставления ордера исходя из имеющегося баланса 
            if (!CheckPossibilityPlacingOrder(orderType)) return;

            //размер заказа в зависимости от решения алгоритма (на продажу размер отрицательный) с учётом настройки ContractValue
            var quantity = orderType == OrderType.ORDER_TYPE_BUY ? _configuration.ContractValue : -_configuration.ContractValue;
            //цена выставляемого ордера в зависимости от решения алгоритма (тип рыночной цены)
            var price = orderType == OrderType.ORDER_TYPE_BUY ? _storage.BuyMarketPrice : _storage.SellMarketPrice;

            //отправляем запрос на биржу выставить ордер по рыночной цене 
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, quantity, _metadata);
            var id = placeResponse.OrderId;
            var response = Converters.ConvertDefaultResponse(placeResponse.Response);

            if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
            {
                //в случае успешного выставления ордера, вносим его в список своих ордеров
                var newOrder = new Order
                {
                    Id = id,
                    Price = price,
                    Quantity = quantity,
                    Signature = new OrderSignature { Status = OrderStatus.ORDER_STATUS_OPEN, Type = orderType },
                    LastUpdateDate = new DateTime()
                };
                var addResponse = _storage.AddOrder(id, newOrder, _storage.MyOrders);
                Log.Information(
                    "{@Where}: Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} added to my orders list {@ResponseCode}",
                    "Former", id, price, quantity, orderType,
                    addResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                //сообщаем сервису истории о новом ордере
                await _historyClient.WriteOrder(newOrder, ChangesType.CHANGES_TYPE_INSERT, _metadata, "Initial order placed");
            }

            Log.Information(
                "{@Where}: Order {@Id} price: {@Price}, quantity: {@Quantity} placed for {@Type} {@ResponseCode} {@ResponseMessage}",
                "Former", id, price, quantity, orderType, response.Code.ToString(),
                response.Code == ReplyCode.REPLY_CODE_FAILURE ? response.Message : "");
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
                var deleteResponse = await _tradeMarketClient.DeleteOrder(key, _metadata);
                var response = Converters.ConvertDefaultResponse(deleteResponse.Response);
                if (response.Code != ReplyCode.REPLY_CODE_SUCCEED)
                {
                    await _historyClient.WriteOrder(value, ChangesType.CHANGES_TYPE_DELETE, _metadata, "Removed by user");
                    return;
                }
                Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", value.Id, value.Price, value.Quantity, value.Signature.Type, response.Code == ReplyCode.REPLY_CODE_SUCCEED ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                
            }
        }

    }
}
