using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradeBot.Relay.RelayService.v1;
using static TradeBot.Relay.RelayService.v1.RelayService;

namespace Facade
{
    public class FacadeRelayService : TradeBot.Relay.RelayService.v1.RelayService.RelayServiceBase
    {
        private RelayServiceClient clientRelay = new RelayServiceClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private readonly ILogger<FacadeRelayService> _logger;
        public FacadeRelayService(ILogger<FacadeRelayService> logger)
        {
            _logger = logger;
        }

        //public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        //{
        //    System.Console.WriteLine("Вызов метода StartBot с параметром: " + request.Config.ToString());

        //    try
        //    {

        //        var response = clientRelay.StartBot(new StartBotRequest { Config = request.Config });

        //        System.Console.WriteLine("Возврат значения из StartBot: " + response.Response.ToString());

        //        return Task.FromResult(new StartBotResponse
        //        {
        //            Response = response.Response
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Ошибка работы метода StarBot");
        //        Console.WriteLine("Exception: "+e.Message);
        //        var defaultResponse = new TradeBot.Common.v1.DefaultResponse
        //        {
        //            Code = TradeBot.Common.v1.ReplyCode.Failure,
        //            Message = "Exception"
        //        };
        //        return Task.FromResult(new StartBotResponse
        //        {
        //            Response = defaultResponse
        //        });
        //    }

        //}

        public override async Task SubscribeLogs(SubscribeLogsRequest request, IServerStreamWriter<SubscribeLogsResponse> responseStream, ServerCallContext context)
        {
            var response = clientRelay.SubscribeLogs(request);

            while (await response.ResponseStream.MoveNext())
            {
                await responseStream.WriteAsync(new SubscribeLogsResponse
                {
                    Response = response.ResponseStream.Current.Response
                });
            }
        }
        /*
         * пока не удалять, мб пригодиться
        public override async Task<SubscribeLogsReply> SubscribeLogs(IAsyncStreamReader<SubscribeLogsRequest> requestStream, ServerCallContext context)
        {
            //var response = clientRelay.StartBot(new StartBotRequest {Log = request.Log });
            await foreach (var item in requestStream.ReadAllAsync())
            {
                
            }
            SubscribeLogsReply subscribeLogsReply = new SubscribeLogsReply();

            subscribeLogsReply.Reply = new DefaultReply { Message = (new Random().Next()).ToString(), Code = ReplyCode.Succeed };
            return subscribeLogsReply;
        }
        */
    }
}

