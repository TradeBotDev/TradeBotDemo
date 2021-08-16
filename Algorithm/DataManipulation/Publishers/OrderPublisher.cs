using System;
using System.Collections.Generic;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public class OrderPublisher
    {
        public delegate void OrderIncoming(Order order);
        public OrderIncoming OrderIncomingEvent;

        public void Publish(Order order)
        {
            OrderIncomingEvent?.Invoke(order);
        }
    }
}
