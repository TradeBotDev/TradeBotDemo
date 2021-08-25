using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using History.DataBase;
using History.DataBase.Data_Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.History.HistoryService.v1;

namespace History
{
    public class History : HistoryService.HistoryServiceBase
    {
        private static readonly ObservableCollection<BalanceChange> BalanceCollection = new();
        private static readonly ObservableCollection<OrderChange> OrderCollection = new();

        public override async Task<PublishEventResponse> PublishEvent(PublishEventRequest request,
            ServerCallContext context)
        {
            await using (var db = new DataContext())
                switch (request.EventTypeCase)
                {
                    case PublishEventRequest.EventTypeOneofCase.None:
                        break;
                    case PublishEventRequest.EventTypeOneofCase.Balance:
                        BalanceWrapper bw = Converter.ToBalanceWrapper(request.Balance.Balance);
                        BalanceChange bc = new BalanceChange
                        {

                            SessionId = request.Balance.Sessionid,
                            Time = request.Balance.Time.ToDateTime(),
                            Balance = bw,
                        };
                        BalanceCollection.Add(bc);
                        db.Add(bw);
                        db.Add(bc);
                        db.SaveChanges();
                        Log.Information("{@Where}: Recorded a change of balance for user {@User}", "History",
                            bc.SessionId);
                        Log.Information("{@Where}: New balance: " + bw.Value + bw.Currency, "History");

                        break;
                    case PublishEventRequest.EventTypeOneofCase.Order:

                        OrderWrapper ow = Converter.ToOrderWrapper(request.Order.Order);
                        OrderChange oc = new OrderChange
                        {
                            SessionId = request.Order.Sessionid,
                            ChangesType = (DataBase.ChangesType)request.Order.ChangesType,
                            Order = ow,
                            Message = request.Order.Message,
                            Time = request.Order.Time.ToDateTime(),
                            SlotName = request.Order.SlotName
                        };
                        OrderCollection.Add(oc);
                        if (oc.ChangesType == ChangesType.CHANGES_TYPE_DELETE && oc.Message != "Removed by user" &&
                            oc.Message != "")
                        {
                            db.Add(ow);
                            db.Add(oc);
                            db.SaveChanges();
                        }

                        Log.Information("{@Where}: Recorded a change of order {@Order}", "History", ow.OrderIdOnTM);
                        Log.Information("{@Where}: Order change: " + oc.Message, "History");

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return new PublishEventResponse();
        }

        private static void Handler(NotifyCollectionChangedEventArgs args, Metadata metadata,
            IAsyncStreamWriter<SubscribeEventsResponse> responseStream)
        {
            BalanceChange updateBalance;
            OrderChange updateOrder;
            Task.Run(async () =>
            {
                if (args.NewItems[0].GetType().FullName.Contains("BalanceChange"))
                {
                    updateBalance = (BalanceChange)args.NewItems[0];
                    if (updateBalance.SessionId != metadata.GetValue("sessionid")) return;
                    await responseStream.WriteAsync(new SubscribeEventsResponse
                    {
                        Balance = new PublishBalanceEvent
                        {
                            Balance = Converter.ToBalance(updateBalance.Balance),
                            Sessionid = updateBalance.SessionId,
                            Time = Timestamp.FromDateTime(new DateTime(updateBalance.Time.Year, updateBalance.Time.Month, updateBalance.Time.Day, 0, 0, 0).ToUniversalTime())
                        }
                    });
                }
                if (args.NewItems[0].GetType().FullName.Contains("OrderChange"))
                {
                    updateOrder = (OrderChange)args.NewItems[0];
                    if (updateOrder.SessionId != metadata.GetValue("sessionid") || updateOrder.SlotName != metadata.GetValue("slot")) return;
                    await responseStream.WriteAsync(new SubscribeEventsResponse
                    {
                        Order = new PublishOrderEvent
                        {
                            ChangesType = (TradeBot.Common.v1.ChangesType)updateOrder.ChangesType,
                            Order = Converter.ToOrder(updateOrder.Order),
                            Sessionid = updateOrder.SessionId,
                            Time = Timestamp.FromDateTime(new DateTime(updateOrder.Time.Year, updateOrder.Time.Month, updateOrder.Time.Day, 0, 0, 0).ToUniversalTime()),
                            Message = updateOrder.Message,
                            SlotName = updateOrder.SlotName
                        }
                    });
                }
            }).Wait();
        }

        public override async Task<SubscribeEventsResponse> SubscribeEvents(SubscribeEventsRequest request,
            IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            var taskCompletionSource = new TaskCompletionSource<SubscribeEventsResponse>();

            void CollectionOnCollectionChanged(object o, NotifyCollectionChangedEventArgs args)
            {
                Handler(args, context.RequestHeaders, responseStream);
            }
            try
            {
                context.CancellationToken.Register(() =>
                {
                    taskCompletionSource.SetCanceled();
                });
                await SendAlreadyPresentData(request.Sessionid, responseStream);
                BalanceCollection.CollectionChanged += CollectionOnCollectionChanged;
                OrderCollection.CollectionChanged += CollectionOnCollectionChanged;
                await taskCompletionSource.Task;
            }
            catch (RpcException e)
            {
                Log.Error("History:" + e.Message);
            }
            finally
            {
                BalanceCollection.CollectionChanged -= CollectionOnCollectionChanged;
                OrderCollection.CollectionChanged -= CollectionOnCollectionChanged;
            }
            return await taskCompletionSource.Task;
        }

        private static List<OrderChange> GetOrderChangesByUser(string user)
        {
            var db = new DataContext();
            return db.OrderRecords.Include(x => x.Order).ToList().FindAll(x => x.SessionId == user);
        }

        private static List<BalanceChange> GetBalanceChangesByUser(string user)
        {
            var db = new DataContext();
            return db.BalanceRecords.Include(x => x.Balance).ToList().FindAll(x => x.SessionId == user);
        }

        private static async Task SendAlreadyPresentData(string sessionId, IAsyncStreamWriter<SubscribeEventsResponse> responseStream)
        {
            var orderChanges = GetOrderChangesByUser(sessionId);
            foreach (var record in orderChanges)
            {
                await responseStream.WriteAsync(new SubscribeEventsResponse
                {
                    Order = new PublishOrderEvent
                    {
                        ChangesType = (TradeBot.Common.v1.ChangesType)ChangesType.CHANGES_TYPE_PARTITIAL,
                        Order = Converter.ToOrder(record.Order),
                        Sessionid = record.SessionId,
                        Time = Timestamp.FromDateTime(new DateTime(record.Time.Year, record.Time.Month, record.Time.Day, 0, 0, 0).ToUniversalTime()),
                        Message = record.Message,
                        SlotName = record.SlotName
                    }
                });
            }
            var balanceChanges = GetBalanceChangesByUser(sessionId);
            if (balanceChanges.Count > 0)
            {
                var currentBalance = balanceChanges.ElementAt(0);
                foreach (var record in balanceChanges)
                {
                    if (record.Time.Year != currentBalance.Time.Year || record.Time.Month != currentBalance.Time.Month || record.Time.Day != currentBalance.Time.Day)
                    {
                        await responseStream.WriteAsync(new SubscribeEventsResponse
                        {
                            Balance = new PublishBalanceEvent
                            {
                                Balance = Converter.ToBalance(currentBalance.Balance),
                                Sessionid = currentBalance.SessionId,
                                Time = Timestamp.FromDateTime(new DateTime(currentBalance.Time.Year, currentBalance.Time.Month, currentBalance.Time.Day, 0, 0, 0).ToUniversalTime())
                            }
                        });
                    }
                    currentBalance = record;
                }
            } 
        }
    }
}
