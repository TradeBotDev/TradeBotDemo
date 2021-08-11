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
                    OrderCollection.Add(new OrderUpdate()
                    {
                        SessionId = request.Order.Sessionid,
                        ChangesType = request.Order.ChangesType,
                        Order = request.Order.Order,
                        Message = request.Order.Message,
                        Time = request.Order.Time
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new PublishEventResponse();
        }

        public override async Task<SubscribeEventsResponse> SubscribeEvents(SubscribeEventsRequest request,
            IServerStreamWriter<SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            var taskCompletionSource = new TaskCompletionSource<SubscribeEventsResponse>();

            BalanceCollection.CollectionChanged += (sender, args) =>
            {
                try
                {
                    var update = (BalanceUpdate)args.NewItems[0];
                    if (update.SessionId == request.Sessionid)
                    {
                        Task.Run(async () =>
                        {
                            await responseStream.WriteAsync(new SubscribeEventsResponse
                            {
                                Balance = new PublishBalanceEvent
                                { Balance = update.Balance, Sessionid = update.SessionId, Time = update.Time }
                            });
                        }).Wait();
                    }
                }
                catch
                {
                    taskCompletionSource.SetCanceled();
                }
            };

            OrderCollection.CollectionChanged += (sender, args) =>
            {
                try
                {
                    var update = (OrderUpdate)args.NewItems[0];
                    if (update.SessionId == request.Sessionid)
                    {
                        Task.Run(async () =>
                        {
                            await responseStream.WriteAsync(new SubscribeEventsResponse
                            {
                                Order = new PublishOrderEvent
                                {
                                    ChangesType = update.ChangesType,
                                    Order = update.Order,
                                    Sessionid = update.SessionId,
                                    Time = update.Time,
                                    Message = update.Message
                                }
                            });
                        }).Wait();
                    }
                }
                catch
                {
                    taskCompletionSource.SetCanceled();
                }
            };

            return await taskCompletionSource.Task;
        }
    }
}
