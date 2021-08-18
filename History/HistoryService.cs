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

        public override async Task<PublishEventResponse> PublishEvent(PublishEventRequest request, ServerCallContext context)
        {
            switch (request.EventTypeCase)
            {
                case PublishEventRequest.EventTypeOneofCase.None:
                    break;
                case PublishEventRequest.EventTypeOneofCase.Balance:
                    using (var db = new DataContext())
                    {
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
                        Log.Information("{@Where}: Recorded a change of balance for user {@User}", "History", bc.SessionId);
                        Log.Information("{@Where}: New balance: " + bw.Value + bw.Currency, "History");
                    }
                    break;
                case PublishEventRequest.EventTypeOneofCase.Order:
                    using (var db = new DataContext())
                    {
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
                        db.Add(ow);
                        db.Add(oc);
                        db.SaveChanges();
                        Log.Information("{@Where}: Recorded a change of order {@Order}", "History", ow.OrderId);
                        Log.Information("{@Where}: Order change: " + oc.ChangesType, "History");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new PublishEventResponse();
        }

        private static void Handler(NotifyCollectionChangedEventArgs args, SubscribeEventsRequest request,
            IAsyncStreamWriter<SubscribeEventsResponse> responseStream,
            TaskCompletionSource<SubscribeEventsResponse> taskCompletionSource)
        {
            BalanceChange updateBalance;
            OrderChange updateOrder;
            try
            {
                Task.Run(async () =>
                {
                    if (args.NewItems[0].GetType().FullName.Contains("BalanceUpdate"))
                    {
                        updateBalance = (BalanceChange)args.NewItems[0];
                        if (updateBalance.SessionId != request.Sessionid) return;
                        await responseStream.WriteAsync(new SubscribeEventsResponse
                        {
                            Balance = new PublishBalanceEvent
                            {
                                Balance = Converter.ToBalance(updateBalance.Balance),
                                Sessionid = updateBalance.SessionId,
                                Time = Timestamp.FromDateTime(updateBalance.Time)
                            }
                        }); ;
                    }
                    if (args.NewItems[0].GetType().FullName.Contains("OrderUpdate"))
                    {
                        updateOrder = (OrderChange)args.NewItems[0];
                        if (updateOrder.SessionId != request.Sessionid) return;
                        await responseStream.WriteAsync(new SubscribeEventsResponse
                        {
                            Order = new PublishOrderEvent
                            {
                                ChangesType = (TradeBot.Common.v1.ChangesType)updateOrder.ChangesType,
                                Order = Converter.ToOrder(updateOrder.Order),
                                Sessionid = updateOrder.SessionId,
                                Time = Timestamp.FromDateTime(updateOrder.Time),
                                Message = updateOrder.Message,
                                SlotName = updateOrder.SlotName
                            }
                        });
                    }
                }).Wait();
            }
            catch
            {
                taskCompletionSource.SetCanceled();
            }
        }

        public override async Task<SubscribeEventsResponse> SubscribeEvents(SubscribeEventsRequest request,
            IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            var taskCompletionSource = new TaskCompletionSource<SubscribeEventsResponse>();

            BalanceCollection.CollectionChanged += (_, args) => Handler(args, request, responseStream, taskCompletionSource);
            List<OrderChange> orderChanges = GetOrderChangesByUser(request.Sessionid);
            foreach (var record in orderChanges)
            {
                await responseStream.WriteAsync(new SubscribeEventsResponse
                {
                    Order = new PublishOrderEvent
                    {
                        ChangesType = (TradeBot.Common.v1.ChangesType)record.ChangesType,
                        Order = Converter.ToOrder(record.Order),
                        Sessionid = record.SessionId,
                        Time = Timestamp.FromDateTime(record.Time.ToUniversalTime()),
                        Message = record.Message,
                        SlotName = record.SlotName
                    }
                });
            }

            OrderCollection.CollectionChanged += (_, args) => Handler(args, request, responseStream, taskCompletionSource);
            List<BalanceChange> balanceChanges = GetBalanceChangesByUser(request.Sessionid);
            foreach (var record in balanceChanges)
            {
                await responseStream.WriteAsync(new SubscribeEventsResponse
                {
                    Balance = new PublishBalanceEvent
                    {
                        Balance = Converter.ToBalance(record.Balance),
                        Sessionid = record.SessionId,
                        Time = Timestamp.FromDateTime(record.Time.ToUniversalTime())
                    }
                }); ;
            }
            return await taskCompletionSource.Task;
        }

        private static List<OrderChange> GetOrderChangesByUser(string user)
        {
            var db = new DataContext();
            return db.OrderRecords.Include(x => x.Order).ToList();
        }

        private static List<BalanceChange> GetBalanceChangesByUser(string user)
        {
            var db = new DataContext();
            return db.BalanceRecords.Include(x => x.Balance).ToList();
        }
    }
}
