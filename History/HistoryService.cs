using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using History.DataBase;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TradeBot.History.HistoryService.v1;

namespace History
{
    public class History : HistoryService.HistoryServiceBase
    {
        private static readonly ObservableCollection<BalanceChange> BalanceCollection = new();
        private static readonly ObservableCollection<OrderChange> OrderCollection = new();
        private static DataContext db = new();

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
                        Balance = request.Balance.Balance
                    });
                    break;
                case PublishEventRequest.EventTypeOneofCase.Order:
                    OrderCollection.Add(new OrderChange()
                    {
                        SessionId = request.Order.Sessionid,
                        ChangesType = request.Order.ChangesType,
                        Order = request.Order.Order,
                        Message = request.Order.Message,
                        Time = request.Order.Time.ToDateTime()
                    }); ;
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
                    var update = (BalanceChange)args.NewItems[0];
                    if (update.SessionId == request.Sessionid)
                    {
                        Task.Run(async () =>
                        {
                            await responseStream.WriteAsync(new SubscribeEventsResponse
                            {
                                Balance = new PublishBalanceEvent
                                { Balance = update.Balance, Sessionid = update.SessionId, Time = Timestamp.FromDateTime(update.Time) }
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
                    var update = (OrderChange)args.NewItems[0];
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
                                    Time = Timestamp.FromDateTime(update.Time),
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
