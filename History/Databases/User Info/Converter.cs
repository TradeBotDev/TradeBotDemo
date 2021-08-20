using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace History.DataBase.Data_Models
{
    public static class Converter
    {
        public static Order ToOrder(OrderWrapper orderWrapper)
        {
            return new Order() { Id = orderWrapper.OrderIdOnTM, LastUpdateDate = Timestamp.FromDateTime(orderWrapper.LastUpdateTime.ToUniversalTime()), Price = orderWrapper.Price, Quantity = orderWrapper.Quantity, Signature = new OrderSignature() { Status = (TradeBot.Common.v1.OrderStatus)orderWrapper.Status, Type = (TradeBot.Common.v1.OrderType)orderWrapper.Type } };
        }

        public static OrderWrapper ToOrderWrapper(Order order)
        {
            return new OrderWrapper() { OrderIdOnTM = order.Id, LastUpdateTime = order.LastUpdateDate.ToDateTime().ToUniversalTime(), Price = order.Price, Quantity = order.Quantity, Status = (OrderStatus)order.Signature.Status, Type = (OrderType)order.Signature.Type };
        }

        public static BalanceWrapper ToBalanceWrapper(Balance balance)
        {
            return new BalanceWrapper() { Currency = balance.Currency, Value = balance.Value };
        }
        
        public static Balance ToBalance(BalanceWrapper balanceWrapper)
        {
            return new Balance() { Currency = balanceWrapper.Currency, Value = balanceWrapper.Value };
        }
    }
}
