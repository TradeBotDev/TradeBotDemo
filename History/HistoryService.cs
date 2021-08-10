using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TradeBot.Common.v1;
using TradeBot.History.HistoryService.v1;

namespace History
{
    public class History : HistoryService.HistoryServiceBase
    {
        struct BalanceUpdate
        {
            public string sessionid;
            public Timestamp time;
            public Balance balance;
        }
        private static ObservableCollection<BalanceUpdate> balanceCollection = new();
        private static ObservableCollection<(string sessionid, Timestamp time, Order order, string message, ChangesType changesType)> orderCollection = new();


        public override async Task<PublishBalanceUpdateResponse> PublishBalanceUpdate(
            PublishBalanceUpdateRequest request,
            ServerCallContext context)
        {
            balanceCollection.Add(new BalanceUpdate
            { sessionid = request.Sessionid, time = request.Time, balance = request.Balance });
            return new PublishBalanceUpdateResponse();
        }

        public override async Task<PublishOrderUpdateResponse> PublishOrderUpdate(PublishOrderUpdateRequest request,
            ServerCallContext context)
        {
            orderCollection.Add((request.Sessionid, request.Time, request.Order, request.Message, request.ChangesType));
            return new PublishOrderUpdateResponse();
        }

        public override async Task<SubscribeBalanceEventsResponse> SubscribeBalanceUpdate(SubscribeBalanceEventsRequest request, IServerStreamWriter<SubscribeBalanceEventsResponse> responseStream,
            ServerCallContext context)
        {

            var taskCompletionSource =
                new TaskCompletionSource<SubscribeBalanceEventsResponse>();
            balanceCollection.CollectionChanged += (sender, args) =>
            {
                try
                {
                    BalanceUpdate b = (BalanceUpdate)args.NewItems[0];
                    Task.Run(async () =>
                    {
                        await responseStream.WriteAsync(new SubscribeBalanceEventsResponse
                            { Sessionid = b.sessionid, Balance = b.balance, Time = b.time });
                    }).Wait();

                }
                catch
                {
                    taskCompletionSource.SetCanceled();
                }
            };

            return await taskCompletionSource.Task;
        }


        public override async Task<SubscribeOrderEventRespones> SubscribeOrderUpdate(SubscribeOrderEventRequest request, IServerStreamWriter<SubscribeOrderEventRespones> responseStream,
            ServerCallContext context)
        {

            return new SubscribeOrderEventRespones();
        }
    }
}
