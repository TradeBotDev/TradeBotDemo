﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Former.Clients;
using Serilog;

namespace Former.Models
{
    public class UpdateHandlers
    {
        private readonly Storage _storage;
        private Configuration _configuration;
        private readonly TradeMarketClient _tradeMarketClient;
        private readonly Metadata _metadata;
        private readonly HistoryClient _historyClient;
        private readonly double _slotMultiplier;
        private double _oldMarketBuyPrice;
        private double _oldMarketSellPrice;
        private int _oldTotalBalance;
        private readonly ILogger _logger;
        
        internal UpdateHandlers(Storage storage, Configuration configuration, TradeMarketClient tradeMarketClient, Metadata metadata, HistoryClient historyClient, double slotMultiplier, ILogger logger)
        {
            _storage = storage;
            _storage.HandleUpdateEvent += MainUpdateHandler;
            _configuration = configuration;
            _metadata = metadata;
            _tradeMarketClient = tradeMarketClient;
            _historyClient = historyClient;
            _slotMultiplier = slotMultiplier;
            _logger = logger.ForContext<UpdateHandlers>();
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
            //так как шаг изменения рыночных цен равен 0,5, то можно было бы написать >= 0,5, но числа с плавающей точкой плохо работают с = и для этого здесь стоит >0,4, которое несёт такую же функцию
            //но не использует =
            if (Math.Abs(_storage.SellMarketPrice - _oldMarketSellPrice) > 0.4 ||
                Math.Abs(_oldMarketBuyPrice - _storage.BuyMarketPrice) > 0.4) await MarketPriceHandleUpdate();
            if (_storage.TotalBalance > 0 && Math.Abs(_oldTotalBalance - _storage.TotalBalance) > 0) await BalanceHandleUpdate();
            if (order is not null) await OrderUpdateHandle(changesType, order);
        }

        private async Task OrderUpdateHandle(ChangesType changesType, Order order)
        {
            //здесь сообщается истории об инициализации оредра или о его удалении (это касается только контр-ордеров)
            if (changesType == ChangesType.CHANGES_TYPE_PARTITIAL)
            {
                _storage.SpentBalance += _storage.LotSize == 1 ? Math.Abs(order.Price * _slotMultiplier) *  order.Quantity : Math.Abs( order.Quantity / order.Price);
                await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_PARTITIAL, Converters.ConvertMetadata(_metadata), "Counter order initialized");
                _logger.ForContext("Method", "OrderUpdateHandle").Information(
                    "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                    order.Id, order.Price, order.Quantity, ChangesType.CHANGES_TYPE_PARTITIAL,
                    "Counter order initialized");
            }

            if (changesType == ChangesType.CHANGES_TYPE_DELETE)
            {
                _storage.SpentBalance -= _storage.LotSize == 1 ? Math.Abs(order.Price * _slotMultiplier *  order.Quantity) : Math.Abs( order.Quantity / order.Price);
                await _historyClient.WriteOrder(FormatDeletedOrder(order), ChangesType.CHANGES_TYPE_DELETE, Converters.ConvertMetadata( _metadata), "Counter order filled");
                _logger.ForContext("Method", "OrderUpdateHandle").Information(
                    "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                    order.Id, order.Price, order.Quantity, ChangesType.CHANGES_TYPE_DELETE,
                    "Counter order filled");
            }
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

        private async Task BalanceHandleUpdate()
        {
            //если баланс изменился, необходимо отправить новый баланс в историю
            _oldTotalBalance = _storage.TotalBalance;
            await _historyClient.WriteBalance(_storage.TotalBalance, Converters.ConvertMetadata(_metadata));
        }

        private async Task MarketPriceHandleUpdate()
        {
            //если рыночная цена изменилась, то необходимо проверить, не устарели ли цени в наших ордерах и подогнать их к рыночной цене 
            //с помощью метода FitPrices
            _oldMarketSellPrice = _storage.SellMarketPrice;
            _oldMarketBuyPrice = _storage.BuyMarketPrice;
            if (!_storage.FitPricesLocker && !_storage.MyOrders.IsEmpty) await FitPrices();
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
                var amendOrderResponse = await _tradeMarketClient.AmendOrder(order.Id, fairPrice, Converters.ConvertMetadata(_metadata));
                var response = Converters.ConvertDefaultResponse(amendOrderResponse.Response);

                if (response.Code == ReplyCode.REPLY_CODE_SUCCEED)
                {
                    //в случае положительного ответа обновляем его в своём списке и сообщаем об изменениях истории
                    UpdateOrderPrice(order, fairPrice);
                    await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_UPDATE, Converters.ConvertMetadata(_metadata), "Order amended");
                    _logger.ForContext("Method", "FitPrices").Information(
                        "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                        order.Id, order.Price, order.Quantity, ChangesType.CHANGES_TYPE_UPDATE,
                        "Order amended");
                }
                else if (response.Message.Contains("Invalid ordStatus"))
                {
                    _storage.SpentBalance -= _storage.LotSize == 1 ? Math.Abs(order.Price * _slotMultiplier *  order.Quantity) : Math.Abs( order.Quantity / order.Price);
                    //при получении ошибки Invalid ordStatus мы понимаем, что пытаемся изменить ордер, которого нет на бирже, 
                    //но при этом он есть у нас в списках, поэтому мы удаляем его из своих списков и сообщаем об удалении истории
                    await _historyClient.WriteOrder(order, ChangesType.CHANGES_TYPE_DELETE, Converters.ConvertMetadata(_metadata), "");
                    _logger.ForContext("Method", "FitPrices").Information(
                        "Order {@Id} price: {@Price}, quantity {@Quantity}, change type {@ChangeType}, message {@Message} sent to history",
                        order.Id, order.Price, order.Quantity, ChangesType.CHANGES_TYPE_DELETE,
                        "");
                    var removeResponse = _storage.RemoveOrder(key, _storage.MyOrders);
                    _logger.ForContext("Method", "FitPrices").Information("My order {@Id}, price: {@Price}, quantity: {@Quantity}, type: {@Type} removed cause cannot be amended {@ResponseCode} ", order.Id, order.Price, order.Quantity, order.Signature.Type, removeResponse ? ReplyCode.REPLY_CODE_SUCCEED : ReplyCode.REPLY_CODE_FAILURE);
                }
                else return;
                _logger.ForContext("Method", "FitPrices").Information("Order {@Id} amended with {@Price} {@ResponseCode} {@ResponseMessage}", key, fairPrice, response.Code.ToString(), response.Code == ReplyCode.REPLY_CODE_SUCCEED ? "" : response.Message);
            }
            _storage.FitPricesLocker = false;
        }
    }
}
