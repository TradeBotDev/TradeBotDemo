using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using History.DataBase;
using History.DataBase.Data_Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using TradeBot.Common.v1;
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
                    BalanceCollection.Add(new BalanceChange
                    {
                        SessionId = request.Balance.Sessionid,
                        Time = request.Balance.Time.ToDateTime(),
                        Balance = Converter.ToBalanceWrapper(request.Balance.Balance)
                    });

                    break;
                case PublishEventRequest.EventTypeOneofCase.Order:
                    OrderCollection.Add(new OrderChange
                    {
                        SessionId = request.Order.Sessionid,
                        ChangesType = (DataBase.ChangesType)request.Order.ChangesType,
                        Order = Converter.ToOrderWrapper(request.Order.Order),
                        Message = request.Order.Message,
                        Time = request.Order.Time.ToDateTime(),
                        SlotName = request.Order.SlotName
                    });

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

            OrderCollection.CollectionChanged += (_, args) => Handler(args, request, responseStream, taskCompletionSource);

            return await taskCompletionSource.Task;
        }
    }
}
