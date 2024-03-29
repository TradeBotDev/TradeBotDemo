﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
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
        private readonly double _slotMultiplier;
        private readonly ILogger _logger;

        internal Former(Storage storage, Configuration configuration, TradeMarketClient tradeMarketClient, Metadata metadata, HistoryClient historyClient, double slotMultiplier, ILogger logger)
        {
            _storage = storage;
            _storage.PlaceOrderEvent += PlaceCounterOrder;
            _configuration = configuration;
            _tradeMarketClient = tradeMarketClient;
            _metadata = metadata;
            _historyClient = historyClient;
            _slotMultiplier = slotMultiplier;
            _logger = logger.ForContext<Former>();
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
            var quantity = Convert.ToInt32(oldOrder.Quantity) != Convert.ToInt32(newComingOrder.Quantity) ? oldOrder.Quantity - newComingOrder.Quantity : oldOrder.Quantity;
            //тип ордера с которым необходимо выставить контр-ордер
            var type = oldOrder.Signature.Type == OrderType.ORDER_TYPE_BUY ? OrderType.ORDER_TYPE_SELL : OrderType.ORDER_TYPE_BUY;
            //цена контр ордера, которая зависит цены старого ордера и типа выставляемого ордера с учётом необходимого профита
            var price = type == OrderType.ORDER_TYPE_BUY
                ? oldOrder.Price - oldOrder.Price * _configuration.RequiredProfit
                : oldOrder.Price + oldOrder.Price * _configuration.RequiredProfit;

            _storage.SpentBalance -= _storage.LotSize == 1 ? Math.Abs(price * _slotMultiplier * quantity) : Math.Abs( quantity / price);
            //ответ при добавлении контр-ордера в соответсвующий список (необходимо для логов)
            var addResponse = false;
            //выставляем контр ордер с рассчитанной ценой и отрицательным числом контрактов, так как контр ордер должен иметь противоположное число 
            //контрактов по сравнению с oldOrder
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, -quantity, Converters.ConvertMetadata(_metadata));
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
                    LastUpdateDate = DateTimeOffset.Now
                };
                addResponse = _storage.AddOrder(placeResponse.OrderId, newOrder, _storage.CounterOrders);
                _storage.SpentBalance += _storage.LotSize == 1 ? Math.Abs(newOrder.Price * _slotMultiplier *  newOrder.Quantity) : Math.Abs( newOrder.Quantity / newOrder.Price);
                //сообщаем о выставлении контр-ордера истории
                await _historyClient.WriteOrder(newOrder, ChangesType.CHANGES_TYPE_INSERT, Converters.ConvertMetadata(_metadata), "Counter order placed");
                _logger.ForContext("Method", "PlaceCounterOrder").Information(
                    "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                     newOrder.Id, newOrder.Price, newOrder.Quantity, ChangesType.CHANGES_TYPE_INSERT,
                    "Counter order placed");
                //сообщаем об исполнении старого ордера истории
                if (Convert.ToInt32(quantity) == Convert.ToInt32(oldOrder.Quantity))
                {
                    
                    await _historyClient.WriteOrder(FormatDeletedOrder(oldOrder), ChangesType.CHANGES_TYPE_DELETE,
                        Converters.ConvertMetadata(_metadata), "Initial order filled");
                    _logger.ForContext("Method", "PlaceCounterOrder").Information(
                        " Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                        oldOrder.Id, oldOrder.Price, oldOrder.Quantity, ChangesType.CHANGES_TYPE_DELETE,
                        "Initial order filled");
                }
                else
                {
                    await _historyClient.WriteOrder(newComingOrder, ChangesType.CHANGES_TYPE_UPDATE, Converters.ConvertMetadata(_metadata),
                        "Initial order partially filled");
                    _logger.ForContext("Method", "PlaceCounterOrder").Information(
                        "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                        newComingOrder.Id, newComingOrder.Price, newComingOrder.Quantity, ChangesType.CHANGES_TYPE_UPDATE,
                        "Initial order partially filled");
                }
            }

            _logger.ForContext("Method", "PlaceCounterOrder").Information(
                "Counter order {@Id} price: {@Price}, quantity: {@Quantity} placed {@ResponseCode} {@ResponseMessage}",
                oldOrder.Id, price, -quantity, placeResponse.Response.Code,
                response.Code == ReplyCode.REPLY_CODE_SUCCEED ? "" : placeResponse.Response.Message);
            _logger.ForContext("Method", "PlaceCounterOrder").Information(
                "Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@ResponseCode} added to counter orders list {@ResponseMessage}",
                placeResponse.OrderId, price, -quantity, type,
                addResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
        }

        private Order FormatDeletedOrder(Order unformattedOrder)
        {
            return new Order
            {
                Id = unformattedOrder.Id, LastUpdateDate = unformattedOrder.LastUpdateDate, Price = unformattedOrder.Price, Quantity = unformattedOrder.Quantity,
                Signature = new OrderSignature
                {
                    Status = OrderStatus.ORDER_STATUS_CLOSED,
                    Type = unformattedOrder.Signature.Type
                }
            };
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type, double quantity, double price)
        {
            if (_storage.LotSize == 0) return false;
            //вычисляем предполагаемую стоимость ордера по рыночной цене в биткоинах
            var orderCost = _storage.LotSize == 1
                ? Math.Abs(price * _slotMultiplier * quantity)
                : Math.Abs(quantity / price);
            //конвертируем баланс в биткоины (XBT), так как он приходит от биржи в сатоши (XBt)
            var allowedBalance = ConvertSatoshiToXBT(_storage.AllowedBalance);
            //проверяем, возможно ли выставить с текущим общим и доступным балансом с учётом настройки
            if (allowedBalance - _storage.SpentBalance > orderCost) return true;
            _logger.ForContext("Method", "CheckPossibilityPlacingOrder").Debug("Cannot place {@Type} order. Insufficient balance.", type);
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
            //размер заказа в зависимости от решения алгоритма (на продажу размер отрицательный) с учётом настройки ContractValue
            var quantity = orderType == OrderType.ORDER_TYPE_BUY ? (_configuration.ContractValue * _storage.LotSize) : -(_configuration.ContractValue * _storage.LotSize);
            //цена выставляемого ордера в зависимости от решения алгоритма (тип рыночной цены)
            var price = orderType == OrderType.ORDER_TYPE_BUY ? _storage.BuyMarketPrice : _storage.SellMarketPrice;

            //проверяем можно ли поставить ордер такого типа в текущей позиции
            if (!CheckPosition(orderType)) return;
            //проверяем возможность выставления ордера исходя из имеющегося баланса 
            if (!CheckPossibilityPlacingOrder(orderType, quantity, price)) return;

            //отправляем запрос на биржу выставить ордер по рыночной цене 
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, quantity, Converters.ConvertMetadata(_metadata));
            var id = placeResponse.OrderId;
            var response = Converters.ConvertDefaultResponse(placeResponse.Response);

            if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
            {
                _storage.SpentBalance += _storage.LotSize == 1 ? Math.Abs(price * _slotMultiplier * quantity) : Math.Abs( quantity / price);
                //в случае успешного выставления ордера, вносим его в список своих ордеров
                var newOrder = new Order
                {
                    Id = id,
                    Price = price,
                    Quantity = quantity,
                    Signature = new OrderSignature { Status = OrderStatus.ORDER_STATUS_OPEN, Type = orderType },
                    LastUpdateDate = DateTimeOffset.Now
                };
                var addResponse = _storage.AddOrder(id, newOrder, _storage.MyOrders);
                _logger.ForContext("Method", "FormOrder").Information(
                    "Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} added to my orders list {@ResponseCode}",
                    id, price, quantity, orderType,
                    addResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                //сообщаем сервису истории о новом ордере
                await _historyClient.WriteOrder(newOrder, ChangesType.CHANGES_TYPE_INSERT,
                    Converters.ConvertMetadata(_metadata), "Initial order placed");
                _logger.ForContext("Method", "FormOrder").Information(
                    "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                    newOrder.Id, newOrder.Price, newOrder.Quantity, ChangesType.CHANGES_TYPE_INSERT,
                    "Initial order placed");
            }

            _logger.ForContext("Method", "FormOrder").Information(
                "Order {@Id} price: {@Price}, quantity: {@Quantity} placed for {@Type} {@ResponseCode} {@ResponseMessage}",
                id, price, quantity, orderType, response.Code.ToString(),
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
                _storage.SpentBalance = 0;
                var deleteResponse = await _tradeMarketClient.DeleteOrder(key, Converters.ConvertMetadata(_metadata));
                var response = Converters.ConvertDefaultResponse(deleteResponse.Response);
                _logger.ForContext("Method", "RemoveAllMyOrders").Information(
                    "My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}",
                    value.Id, value.Price, value.Quantity, value.Signature.Type,
                    response.Code == ReplyCode.REPLY_CODE_SUCCEED
                        ? ReplyCode.REPLY_CODE_SUCCEED
                        : ReplyCode.REPLY_CODE_FAILURE);
                if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
                {
                    _storage.MyOrders.TryRemove(key, out _);
                    await _historyClient.WriteOrder(value, ChangesType.CHANGES_TYPE_DELETE, Converters.ConvertMetadata(_metadata),
                        "Removed by user");
                    _logger.ForContext("Method", "RemoveAllMyOrders").Information(
                        "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                        value.Id, value.Price, value.Quantity, ChangesType.CHANGES_TYPE_DELETE,
                        "Removed by user");
                }
                else return;
            }
        }

    }
}
