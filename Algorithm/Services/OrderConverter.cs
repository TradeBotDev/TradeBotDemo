using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Algorithm.Services
{
    public static class OrderConverter
    {
        public static OrderWrapper WrapOrder(Order order)
        {
            return new OrderWrapper { id = order.Id, LastUpdateDate = order.LastUpdateDate.ToDateTime(), orderStatus = (OrderStatus)order.Signature.Status, orderType = (OrderType)order.Signature.Type, price = order.Price, quantity = order.Quantity };
        }

        public static Order UnwrapOrder(OrderWrapper wrappedOrder)
        {
            return new Order { Id = wrappedOrder.id, LastUpdateDate = Timestamp.FromDateTime(wrappedOrder.LastUpdateDate.ToUniversalTime()), Price = wrappedOrder.price, Quantity = wrappedOrder.quantity, Signature = new OrderSignature { Status = (TradeBot.Common.v1.OrderStatus)wrappedOrder.orderStatus, Type = (TradeBot.Common.v1.OrderType)wrappedOrder.orderType } };
        }
    }
}
