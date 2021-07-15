using Algorithm.Former;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeMarket.Former;

namespace Former.Services
{

    //public delegate void EventDelegate();
    //public class Events
    //{
    //    public event EventDelegate Event = null;
    //    public void InvokeEvent()
    //    {
    //        Event.Invoke();
    //    }
    //}

    public class Observer
    {
        
        public async void ObserveAlgorithm()
        {
            var algorithmClient = new AlgorithmObserverService.AlgorithmObserverServiceClient (Channels.AlgorithmChannel);
            await Task.Delay(2000);
            using var call = algorithmClient.SubscribePurchasePrice(new SubscribePurchasePriceRequest());

            while (await call.ResponseStream.MoveNext())
            {
                Former.FormShoppingList(call.ResponseStream.Current.PurchasePrice);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }
        public async void ObserveTradeMarket()
        {
            var tradeMarketClient = new FormerService.FormerServiceClient(Channels.TradeMarketChannel);
            await Task.Delay(2000);
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };
            var request = new SubscribeOrdersRequest()
            {
                Signature = orderSignature
            };
            using var call = tradeMarketClient.SubscribeOrders(request);
            while (await call.ResponseStream.MoveNext())
            {
                Former.UpdateCurrentOrders(call.ResponseStream.Current);
            }
            //TODO выход из цикла и дальнейшее закрытие канала
        }
    }
}
