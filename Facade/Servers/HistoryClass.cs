using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using History = TradeBot.History.HistoryService.v1.HistoryService.HistoryServiceClient;
using Ref = TradeBot.Facade.FacadeService.v1;
namespace Facade
{
    public class HistoryClass
    {
        private GrpcChannel _channel => GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("HISTORY_CONNECTION_STRING"));
        public GrpcChannel Channel { get => _channel; }

        private History _client => new History(Channel);
        public History Client
        {
            get => _client;
        }
        public async Task History_SubscribeEvents(Ref.SubscribeEventsRequest request, IServerStreamWriter<Ref.SubscribeEventsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                using var response = Client.SubscribeEvents(new TradeBot.History.HistoryService.v1.SubscribeEventsRequest { Sessionid = request.Sessionid }, context.RequestHeaders);
                Log.Information("{@Where}: {@MethodName} \n args: request={@request}", "Facade", nameof(History_SubscribeEvents), request.Sessionid);
                while (await response.ResponseStream.MoveNext(context.CancellationToken))
                {
                    if (context.CancellationToken.IsCancellationRequested) throw new Exception();
                    switch (response.ResponseStream.Current.EventTypeCase)
                    {
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.Balance:
                            Log.Information("{@Where}: {@MethodName} \n args: Balance={@response}", "Facade", nameof(History_SubscribeEvents), response.ResponseStream.Current.Balance.Balance);
                            await responseStream.WriteAsync(new Ref.SubscribeEventsResponse
                            {
                                Balance = new Ref.PublishBalanceEvent
                                {
                                    Sessionid = response.ResponseStream.Current.Balance.Sessionid,
                                    Balance = response.ResponseStream.Current.Balance.Balance,
                                    Time = response.ResponseStream.Current.Balance.Time
                                }
                            });
                            break;
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.Order:
                            Log.Information("{@Where}: {@MethodName} \n args: Order={@response}", "Facade", nameof(History_SubscribeEvents), response.ResponseStream.Current.Order.Order);
                            Log.Information("{@Where}: {@MethodName} \n args: Message={@response}", "Facade", nameof(History_SubscribeEvents), response.ResponseStream.Current.Order.Message);
                            await responseStream.WriteAsync(new Ref.SubscribeEventsResponse
                            {
                                Order = new Ref.PublishOrderEvent
                                {
                                    ChangesType = response.ResponseStream.Current.Order.ChangesType,
                                    Message = response.ResponseStream.Current.Order.Message,
                                    Sessionid = response.ResponseStream.Current.Order.Sessionid,
                                    Order = response.ResponseStream.Current.Order.Order,
                                    Time = response.ResponseStream.Current.Order.Time,
                                    SlotName = response.ResponseStream.Current.Order.SlotName
                                }
                            });
                            break;
                        case TradeBot.History.HistoryService.v1.SubscribeEventsResponse.EventTypeOneofCase.None:
                            Log.Information("{@Where}: {@MethodName} Get none", "Facade", nameof(History_SubscribeEvents));
                            break;
                    }
                }


            }
            catch (Exception e)
            {
                Log.Error("{@Where}: {@MethodName} Exception: {@Exception}","Facade",nameof(History_SubscribeEvents),e.Message);
                throw;
            }
        }
    }
}
