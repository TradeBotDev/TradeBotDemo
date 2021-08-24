using Algorithm.Services;
using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Algorithm.DataManipulation
{
    public class OrderPublisher
    {
        public delegate void OrderIncoming(OrderWrapper order, Metadata metadata);
        public OrderIncoming OrderIncomingEvent;

        public void Publish(OrderWrapper order, Metadata metadata)
        {
            OrderIncomingEvent?.Invoke(order, metadata);
        }
    }
}
