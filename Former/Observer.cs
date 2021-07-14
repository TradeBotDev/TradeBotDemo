using Grpc.Core;
using Grpc.Net.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeMarket.Former;
using Algorithm.Former;
using System.Threading;
using Google.Protobuf.Collections;
using AutoMapper;
using System;

namespace Former.Services
{

    public delegate void EventDelegate();
    public class Events
    {
        public event EventDelegate Event = null;
        public void InvokeEvent()
        {
            Event.Invoke();
        }
    }
    
    public class Observer
    {
        static double AveragePrice = 0;
        static List<SubscribeOrdersReply> CurrentBuyOrders = new List<SubscribeOrdersReply>();
        static GrpcChannel AlgorithmChannel;
        static GrpcChannel TradeMarketChannel;
        static FormerService.FormerServiceClient TradeMarketClient;
        static AlgorithmObserverService.AlgorithmObserverServiceClient AlgorithmClient;
               
        public Observer() 
        {
            AlgorithmChannel = GrpcChannel.ForAddress("https://localhost:5001");
            TradeMarketChannel = GrpcChannel.ForAddress("https://localhost:5005");
            
            AlgorithmClient = new AlgorithmObserverService.AlgorithmObserverServiceClient(AlgorithmChannel);
            TradeMarketClient = new FormerService.FormerServiceClient(TradeMarketChannel);
        }

        public class ReplyComparer : IComparer<SubscribeOrdersReply>
        {
            int IComparer<SubscribeOrdersReply>.Compare(SubscribeOrdersReply x, SubscribeOrdersReply y)
            {
                return x.SimpleOrderInfo.Price.CompareTo(y.SimpleOrderInfo.Price);
            }
        }

        public static void Sort(List<SubscribeOrdersReply> list, IComparer<SubscribeOrdersReply> comparer)
        {
            for (int i = list.Count - 1; i > 0; i--)
                for (int j = 1; j <= i; j++)
                {
                    var element1 = list[j - 1];
                    var element2 = list[j];
                    if (comparer.Compare(element1, element2) > 0)
                    {
                        var temporary = list[j];
                        list[j] = list[j - 1];
                        list[j - 1] = temporary;
                    }
                }
        }

        private void EventHandler()
        {
            List<string> formedShoppingListById = FormShoppingList(CurrentBuyOrders);
            //SendShopingListToTM(formedShoppingListById);
        }

        private async void SendShopingListToTM(List<string> formedShoppingList) 
        {
            var sendOrderClient = new FormerService.FormerServiceClient(TradeMarketChannel);
            using var call = sendOrderClient.BuyOrder();

            var readTask = Task.Run(async ()=> 
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (response.Reply.Code != 0) Console.WriteLine(response.Reply.Message);
                }
            });
            foreach (var id in formedShoppingList) 
            {
                await call.RequestStream.WriteAsync(new BuyOrderRequest() { Id = id });
            }
            await call.RequestStream.CompleteAsync();
            await readTask;
        }

        private List<string> FormShoppingList(List<SubscribeOrdersReply> currentBuyOrders) 
        {
            List<string> formedShoppingListById = new List<string>();
            foreach (var order in currentBuyOrders)
            {
                if (order.SimpleOrderInfo.Price <= AveragePrice) formedShoppingListById.Add(order.Id);
            }
            return formedShoppingListById;
        }

        public async void ObserveAlgorithm()
        {
            await Task.Delay(2000);
            Events instance = new Events();
            instance.Event += new EventDelegate(EventHandler);
            using var call = AlgorithmClient.SubscribePurchasePrice(new SubscribePurchasePriceRequest());

            while (await call.ResponseStream.MoveNext())
            {
                AveragePrice = call.ResponseStream.Current.PurchasePrice;
                instance.InvokeEvent();
                Console.WriteLine("Принял" + AveragePrice);
            }
            AlgorithmChannel.Dispose();
        }

        public async void ObserveTradeMarket()
        {
            Events instance = new Events();
            instance.Event += new EventDelegate(EventHandler);
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

            using var call = TradeMarketClient.SubscribeOrders(request);

            while (await call.ResponseStream.MoveNext())
            {
                UpdateCurrentBuyOrders(call.ResponseStream.Current);
                Console.WriteLine("Принял" + call.ResponseStream.Current);
            }
            TradeMarketChannel.Dispose();
        }

        private void UpdateCurrentBuyOrders(SubscribeOrdersReply orderNeededUpdate)
        {
            if (CurrentBuyOrders.FindAll(x => x.Id == orderNeededUpdate.Id).Count != 0)
            {
                int updatedIndex = CurrentBuyOrders.FindIndex(x => x.Id == orderNeededUpdate.Id);
                CurrentBuyOrders.RemoveAt(updatedIndex);
                CurrentBuyOrders.Insert(updatedIndex, orderNeededUpdate);
                Sort(CurrentBuyOrders, new ReplyComparer());
            }
            else
            {
                if (CurrentBuyOrders.Count == 9)
                {
                    CurrentBuyOrders.Sort();
                    CurrentBuyOrders.RemoveAt(9);
                    CurrentBuyOrders.Add(orderNeededUpdate);
                    Sort(CurrentBuyOrders, new ReplyComparer());
                }
                else
                {
                    CurrentBuyOrders.Add(orderNeededUpdate);
                    Sort(CurrentBuyOrders, new ReplyComparer());
                }
            }
        }
    }
}
