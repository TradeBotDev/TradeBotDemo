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

        public class ReplyComparator : IComparer<SubscribeOrdersReply>
        {
            int IComparer<SubscribeOrdersReply>.Compare(SubscribeOrdersReply x, SubscribeOrdersReply y)
            {
                return x.SimpleOrderInfo.Price.CompareTo(y.SimpleOrderInfo.Price);
            }
        }
        //private void EventHandler()
        //{
        //    List<string> formedShoppingListById = FormShoppingList(CurrentBuyOrders);
        //    //SendShopingListToTM(formedShoppingListById);
        //}
        private void EventHandler()
        {
            List<string> formedShoppingListById = FormShoppingList(CurrentBuyOrders);
            //SendShopingListToTM(formedShoppingListById);
            //return formedShoppingListById;
            Console.Write("\nСформировал список необходимых ордеров: \n{ ");
            foreach (var elem in formedShoppingListById) 
            {
                Console.Write(elem);
                Console.Write(", ");
            }
            Console.WriteLine("}");
        }


        private async void SendShopingListToTM(List<string> formedShoppingList)
        {
            var sendOrderClient = new FormerService.FormerServiceClient(TradeMarketChannel);
            using var call = sendOrderClient.BuyOrder();

            var readTask = Task.Run(async () =>
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
            //Events instance = new Events();
            //instance.Event += new EventDelegate(EventHandler);
            using var call = AlgorithmClient.SubscribePurchasePrice(new SubscribePurchasePriceRequest());

            while (await call.ResponseStream.MoveNext())
            {
                AveragePrice = call.ResponseStream.Current.PurchasePrice;
                //instance.InvokeEvent();
                EventHandler();
                Console.WriteLine("Принял от алгоритма среднюю цену: " + AveragePrice);
            }
            AlgorithmChannel.Dispose();
        }

        public async void ObserveTradeMarket()
        {
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
                Console.WriteLine("Принял от маркета заказ {0}: цена {1}, количество {2}", call.ResponseStream.Current.Id, call.ResponseStream.Current.SimpleOrderInfo.Price, call.ResponseStream.Current.SimpleOrderInfo.Quantity);
            }
            TradeMarketChannel.Dispose();
        }

        private async void UpdateCurrentBuyOrders(SubscribeOrdersReply orderNeededUpdate)
        {
            var task = Task.Run(() =>
            {
                if (CurrentBuyOrders.FindAll(x => x.Id == orderNeededUpdate.Id).Count != 0)
                {
                    int updatedIndex = CurrentBuyOrders.FindIndex(x => x.Id == orderNeededUpdate.Id);
                    CurrentBuyOrders.RemoveAt(updatedIndex);
                    CurrentBuyOrders.Insert(updatedIndex, orderNeededUpdate);
                    Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                }
                else
                {
                    //TODO разобраться с размером стакана цен
                    if (CurrentBuyOrders.Count == 9)
                    {
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                        CurrentBuyOrders.RemoveAt(9);
                        CurrentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                    }
                    else
                    {
                        CurrentBuyOrders.Add(orderNeededUpdate);
                        Array.Sort(CurrentBuyOrders.ToArray(), new ReplyComparator());
                    }
                }
            });
            await task;
        }
    }
}
