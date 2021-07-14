using System;
using System.Collections.Generic;
using TradeBot.Former.FormerService.v1;

namespace Former
{
    public delegate void EventDelegateForUpdating(SubscribeOrdersResponse reply);
    public delegate void EventDelegateForSending(List<string> list);
    public delegate void EventDelegateForForming(double num);

    public class Events
    {
        public event EventDelegateForUpdating UpdatingCurrentBuyOrders = null;
        public event EventDelegateForForming FormingList = null;
        public event EventDelegateForSending SendingFormedList = null;

        public void InvokeUpdating(SubscribeOrdersResponse reply)
        {
            UpdatingCurrentBuyOrders.Invoke(reply);
        }
        public void InvokeForming(double num)
        {
            FormingList.Invoke(num);
        }
        public void InvokeSending(List<string> list)
        {
            SendingFormedList.Invoke(list);
        }
    }
}
