using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeBot.Relay.StarterService.v1;
using static TradeBot.Relay.StarterService.v1.Starter;

namespace Facade
{
    public class FacadeRelayService : TradeBot.Relay.StarterService.v1.Starter.StarterBase
    {
        private StarterClient clientRelay = new StarterClient(GrpcChannel.ForAddress("https://localhost:5004"));
        private readonly ILogger<FacadeRelayService> _logger;
        public FacadeRelayService(ILogger<FacadeRelayService> logger)
        {
            _logger = logger;
        }

        public override Task<StartBotResponse> StartBot(StartBotRequest request, ServerCallContext context)
        {
            var response = clientRelay.StartBot(new StartBotRequest {Config = request.Config });
            return Task.FromResult(new StartBotResponse
            {
                Response = response.Response 
            });
        }
        /*
         * пока не удалять мб пригодиться
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

