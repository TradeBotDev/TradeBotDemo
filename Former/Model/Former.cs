using System;
using Former.Clients;
using Grpc.Core;
using Serilog;
using System.Threading.Tasks;

using System.Linq;

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
        /// <summary>
        /// Обвновляет конфигурацию в Former (позволяет изменять конфигурацию во время работы)
        /// </summary>
        internal void SetConfiguration(Config configuration)
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
            var type = oldOrder.Signature.Type == OrderType.Buy ? OrderType.Sell : OrderType.Buy;
            //цена контр ордера, которая зависит цены старого ордера и типа выставляемого ордера с учётом необходимого профита
            var price = type == OrderType.Buy
                ? oldOrder.Price - oldOrder.Price * _configuration.RequiredProfit
                : oldOrder.Price + oldOrder.Price * _configuration.RequiredProfit;

            //ответ при добавлении контр-ордера в соответсвующий список (необходимо для логов)
            var addResponse = false;
            //выставляем контр ордер с рассчитанной ценой и отрицательным числом контрактов, так как контр ордер должен иметь противоположное число 
            //контрактов по сравнению с oldOrder
            var placeResponse = await _tradeMarketClient.PlaceOrder(price, -quantity, _metadata);
            if (placeResponse.Response.Code == ReplyCode.Succeed)
            {
                //в случае положительного ответа от биржи вносим контр-ордер в соответсвующий список с полученным в ответе OrderId
                var newOrder = new Order
                {
                    Id = placeResponse.OrderId,
                    Price = price,
                    //Поставил минус
                    Quantity = -quantity,
                    Signature = new OrderSignature { Status = OrderStatus.Open, Type = type },
                    LastUpdateDate = new Timestamp()
                };
                addResponse = _storage.AddOrder(placeResponse.OrderId, newOrder, _storage.CounterOrders);

                //сообщаем о выставлении контр-ордера истории
                await _historyClient.WriteOrder(newOrder, ChangesType.Insert, _metadata, "Counter order placed");

                //сообщаем об исполнении старого ордера истории
                if (Convert.ToInt32(quantity) == Convert.ToInt32(oldOrder.Quantity))
                    await _historyClient.WriteOrder(oldOrder, ChangesType.Delete, _metadata, "Initial order filled");
                else
                    await _historyClient.WriteOrder(newComingOrder, ChangesType.Update, _metadata,
                        "Initial order partially filled");
            }

            Log.Information(
                "{@Where}: Counter order {@Id} price: {@Price}, quantity: {@Quantity} placed {@ResponseCode} {@ResponseMessage}",
                "Former", oldOrder.Id, price, -quantity, placeResponse.Response.Code,
                placeResponse.Response.Code == ReplyCode.Succeed ? "" : placeResponse.Response.Message);
            Log.Information(
                "{@Where}: Order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@ResponseCode} added to counter orders list {@ResponseMessage}",
                "Former", placeResponse.OrderId, price, -quantity, type,
                addResponse ? ReplyCode.Succeed : ReplyCode.Failure);
        }

        /// <summary>
        /// Возвращает false, если текущий баланс и маржа не позволяют выставить ордер на покупку/продажу, иначе true
        /// </summary>
        private bool CheckPossibilityPlacingOrder(OrderType type)
        {
            
            //вычисляем предполагаемую стоимость ордера по рыночной цене в биткоинах
            var orderCost = _configuration.ContractValue / (type == OrderType.Sell ? _storage.SellMarketPrice : _storage.BuyMarketPrice);
            //конвертируем баланс в биткоины (XBT), так как он приходит от биржи в сатоши (XBt)
            var totalBalance = ConvertSatoshiToXBT(_storage.TotalBalance);
            var availableBalance = ConvertSatoshiToXBT(_storage.AvailableBalance);
            //проверяем, возможно ли выставить с текущим общим и доступным балансом с учётом настройки
            if (totalBalance * (_configuration.AvaibleBalance - 1) + availableBalance > orderCost) return true;
            Log.Debug("{@Where}: Cannot place {@Type} order. Insufficient balance.", "Former", type);
            return false;
        }

        /// <summary>
        /// Если у нас уже есть ордера на этого типа, запрещаем ставить ордер противоположного типа во избежание неверного подсчёта доступного баланса.
        /// </summary>
        private bool CheckPosition(OrderType type)
        {
            //проверяет, есть ли уже ордера противоположного типа и возвращает false, если таковые имеются.
            var alreadyPresentOrderTypes = type == OrderType.Sell ? OrderType.Buy : OrderType.Sell;
            if (_storage.MyOrders.Count(x => x.Value.Signature.Type == alreadyPresentOrderTypes) > 0 ||
                _storage.CounterOrders.Count(x => x.Value.Signature.Type == alreadyPresentOrderTypes) > 0 )
                return false;
            return true;
        }

        /// <summary>
        /// Формирует в зависимости от решения алгоритма
        /// </summary>
        internal async Task FormOrder(int decision)
        {
            if (_storage.PlaceLocker) return;
            //тип выставляемого ордера в зависимости от решения алгоритма
            var orderType = decision > 0 ? OrderType.Buy : OrderType.Sell;

            //проверяем можно ли поставить ордер такого типа в текущей позиции
            if (!CheckPosition(orderType)) return;
            //проверяем возможность выставления ордера исходя из имеющегося баланса 
            if (!CheckPossibilityPlacingOrder(orderType)) return;

            //размер заказа в зависимости от решения алгоритма (на продажу размер отрицательный) с учётом настройки ContractValue
            var quantity = orderType == OrderType.Buy ? _configuration.ContractValue : -_configuration.ContractValue;
            //цена выставляемого ордера в зависимости от решения алгоритма (тип рыночной цены)
            var price = orderType == OrderType.Buy ? _storage.BuyMarketPrice : _storage.SellMarketPrice;

            //отправляем запрос на биржу выставить ордер по рыночной цене 
            var response = await _tradeMarketClient.PlaceOrder(price, quantity, _metadata);
            if (response.Response.Code == ReplyCode.Succeed)
            {
                //в случае успешного выставления ордера, вносим его в список своих ордеров
                var newOrder = new Order
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
                //сообщаем сервису истории о новом ордере
                await _historyClient.WriteOrder(newOrder, ChangesType.Insert, _metadata, "Initial order placed");
            }

            Log.Information(
                "{@Where}: Order {@Id} price: {@Price}, quantity: {@Quantity} placed for {@Type} {@ResponseCode} {@ResponseMessage}",
                "Former", response.OrderId, price, quantity, orderType, response.Response.Code.ToString(),
                response.Response.Code == ReplyCode.Failure ? response.Response.Message : "");
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
                if (response.Response.Code != ReplyCode.Succeed)
                {
                    await _historyClient.WriteOrder(value, ChangesType.Delete, _metadata, "Removed by user");
                    return;
                }
                Log.Information("{@Where}: My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed {@ResponseCode}", "Former", value.Id, value.Price, value.Quantity, value.Signature.Type, response.Response.Code == ReplyCode.Succeed ? ReplyCode.Succeed : ReplyCode.Failure);
                
            }
        }

    }
}
