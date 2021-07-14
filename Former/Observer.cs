using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Former.FormerService.v1;
using TradeBot.Common.v1;
using SubscribeOrdersRequest = TradeBot.Former.FormerService.v1.SubscribeOrdersRequest;

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
            var algorithmClient = new FormerService.FormerServiceClient(Channels.AlgorithmChannel);
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
            var orderSignature = new OrderSignature
            {
                Status = OrderStatus.Open,
                Type = OrderType.Buy
            };
            var request = new SubscribeOrdersRequest()
            {
                Request = orderSignature
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
