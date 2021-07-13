using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using System.Threading;
using Grpc.Core;

namespace Algorithm
{

    //public delegate void EventDelegade();
    //public class Events
    //{
    //    public event EventDelegade Event = null;

    //    void InvokeEvent()
    //    {
    //        Event.Invoke();
    //    }
    //}
    public class AlgorithmEmulator : IAlgorithm
    {
        public double CalculateSuggestedPrice()
        {
            Random rn = new Random();
            Thread.Sleep(rn.Next(0, 10000));
            return 10;
        }

        public static void SendData(IServerStreamWriter<SubscribePurchasePriceReply> sw, double price)
        {
            sw.WriteAsync(new SubscribePurchasePriceReply { PurchasePrice = price });
        }

    }

    
}
