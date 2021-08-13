using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
        private struct BalanceUpdate
        {
            public string SessionId;
            public Timestamp Time;
            public Balance Balance;
        }
        private struct OrderUpdate
        {
            public string SessionId;
            public Timestamp Time;
            public Order Order;
            public string Message;
            public ChangesType ChangesType;
            public string SlotName;
        }

        private static readonly ObservableCollection<BalanceUpdate> BalanceCollection = new();
        private static readonly ObservableCollection<OrderUpdate> OrderCollection = new();


        public override async Task<PublishEventResponse> PublishEvent(PublishEventRequest request, ServerCallContext context)
        {
            switch (request.EventTypeCase)
            {
                case PublishEventRequest.EventTypeOneofCase.None:
                    break;
                case PublishEventRequest.EventTypeOneofCase.Balance:
                    BalanceCollection.Add(new BalanceUpdate
                    {
                        SessionId = request.Balance.Sessionid,
                        Time = request.Balance.Time,
                        Balance = request.Balance.Balance
                    });
                    break;
                case PublishEventRequest.EventTypeOneofCase.Order:
                    OrderCollection.Add(new OrderUpdate
                    {
                        SessionId = request.Order.Sessionid,
                        ChangesType = request.Order.ChangesType,
                        Order = request.Order.Order,
                        Message = request.Order.Message,
                        Time = request.Order.Time,
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
            BalanceUpdate updateBalance;
            OrderUpdate updateOrder;
            try
            {
                Task.Run(async () =>
                {
                    if (args.NewItems[0].GetType().FullName.Contains("BalanceUpdate"))
                    {
                        updateBalance = (BalanceUpdate)args.NewItems[0];
                        if (updateBalance.SessionId != request.Sessionid) return;
                        await responseStream.WriteAsync(new SubscribeEventsResponse
                        {
                            Balance = new PublishBalanceEvent
                            {
                                Balance = updateBalance.Balance, 
                                Sessionid = updateBalance.SessionId,
                                Time = updateBalance.Time
                            }
                        });
                    }
                    if (args.NewItems[0].GetType().FullName.Contains("OrderUpdate"))
                    {
                        updateOrder = (OrderUpdate)args.NewItems[0];
                        if (updateOrder.SessionId != request.Sessionid) return;
                        await responseStream.WriteAsync(new SubscribeEventsResponse
                        {
                            Order = new PublishOrderEvent
                            {
                                ChangesType = updateOrder.ChangesType,
                                Order = updateOrder.Order,
                                Sessionid = updateOrder.SessionId,
                                Time = updateOrder.Time,
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
