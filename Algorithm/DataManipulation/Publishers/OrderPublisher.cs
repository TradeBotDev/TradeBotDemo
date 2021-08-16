using Grpc.Core;
using System;
using System.Collections.Generic;
using TradeBot.Common.v1;

namespace Algorithm.DataManipulation
{
    public class OrderPublisher
    {
        public delegate void OrderIncoming(Order order, Metadata metadata);
        public OrderIncoming OrderIncomingEvent;

        public void Publish(Order order, Metadata metadata)
        {
            OrderIncomingEvent?.Invoke(order, metadata);
        }
    }
}
