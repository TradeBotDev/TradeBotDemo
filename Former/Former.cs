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
        

        private bool _placeLocker;

        private bool _fitPricesLocker;

        /// <summary>
        /// Выставляет контр-ордер в полном объёме от изначального ордера
        /// </summary>
        internal async Task<PlaceOrderResponse> PlaceFullCounterOrder(double price, double quantity, UserContext context)
        {
            var response = await context.PlaceOrder(price, -quantity);
            return response;
        }

        /// <summary>
        /// Выставляет контр-ордер с частью объёма от изначального ордера
        /// </summary>
        internal async Task<PlaceOrderResponse> PlacePartialCounterOrder(double price, double newQuantity, double oldQuantity, UserContext context)
        {
            var quantity = oldQuantity - newQuantity;
            var response = await context.PlaceOrder(price, -quantity);
            return response;
        }

        /// <summary>
        /// Формирует ордер на покупку
        /// </summary>
        internal async Task FormPurchaseOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            double quantity;
            var availableBalance = ConvertSatoshiToXbt(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalance = ConvertSatoshiToXbt(_totalBalance) * context.Configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            var purchaseFairPrice = _buyOrderBook.Max(x => x.Value.Price);

            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать только по доступному балансу
            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize >= 0) quantity = context.Configuration.ContractValue * Math.Floor(availableBalance * purchaseFairPrice / context.Configuration.ContractValue);
            else quantity = context.Configuration.ContractValue * Math.Floor(totalBalance * purchaseFairPrice / context.Configuration.ContractValue) - _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                var response = await context.PlaceOrder(purchaseFairPrice - 1, quantity);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = purchaseFairPrice - 1,
                        Quantity = quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Buy
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                    _positionSizeInActiveOrders += (int)quantity;
                }
                Log.Information("Order price: {0}, quantity: {1} placed for purchase {2} {3}", purchaseFairPrice - 1, quantity, response.Response.Code.ToString(), response.Response.Message);
            }
            else Log.Debug("Cannot place purchase order. Insufficient balance.");
        }

        /// <summary>
        /// Формирует ордер на продажу
        /// </summary>
        internal async Task FormSellOrder(UserContext context)
        {
            if (_placeLocker) return;
            if (CheckContext(context)) return;
            double quantity;
            var availableBalance = ConvertSatoshiToXbt(_availableBalance) * context.Configuration.AvaibleBalance;
            var totalBalance = ConvertSatoshiToXbt(_totalBalance) * context.Configuration.AvaibleBalance;
            //вычисляем рыночную цену для продажи
            var sellFairPrice = _sellOrderBook.Min(x => x.Value.Price);

            //если наша позиция короткая, то есть _positionSize имеет отрицательное значение, то мы можем продавать только по доступному балансу
            //если наша позиция длинная, то есть _positionSize имеет положительное значение, то мы можем продавать по общему балансу, который есть на аккаунте
            if (_positionSize <= 0) quantity = context.Configuration.ContractValue * Math.Floor(availableBalance * sellFairPrice / context.Configuration.ContractValue);
            else quantity = context.Configuration.ContractValue * Math.Floor(totalBalance * sellFairPrice / context.Configuration.ContractValue) + _positionSizeInActiveOrders;

            //если баланса было достаточно для хотя бы одного ордера то выполняем продажу
            if (quantity > 0)
            {
                var response = await context.PlaceOrder(sellFairPrice + 1, -quantity);
                if (response.Response.Code == ReplyCode.Succeed)
                {
                    _myOrders.TryAdd(response.OrderId, new Order
                    {
                        Id = response.OrderId,
                        Price = sellFairPrice + 1,
                        Quantity = -quantity,
                        Signature = new OrderSignature
                        {
                            Status = OrderStatus.Open,
                            Type = OrderType.Sell
                        },
                        LastUpdateDate = new Google.Protobuf.WellKnownTypes.Timestamp()
                    });
                }
                Log.Information("Order price: {0}, quantity: {1} placed for sell {2} {3}", sellFairPrice + 1, -quantity, response.Response.Code.ToString(), response.Response.Message);
                _positionSizeInActiveOrders -= (int)quantity;
            }
            else Log.Debug("Cannot place sell order. Insufficient balance.");
        }

        /// <summary>
        /// Конвертирует сатоши в биткоины
        /// </summary>
        private double ConvertSatoshiToXbt(int satoshiValue)
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
