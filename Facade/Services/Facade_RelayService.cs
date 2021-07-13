using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;
using TradeBot.Relay.Facade;
using static TradeBot.Relay.Facade.SendToRelay;

namespace Facade
{
    public class SendToRelay : TradeBot.Relay.Facade.SendToRelay.SendToRelayBase
    {
        //private SendToRelayClient clientRelay = new SendToRelayClient(GrpcChannel.ForAddress(""/*адрес ТМ*/));
        private readonly ILogger<UIInfoService> _logger;
        public SendToRelay(ILogger<UIInfoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Запускает бота
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<StartBotReply> StartBot(StartBotRequest request, ServerCallContext context)
        {
            //var response = clientRelay.StartBot(new StartBotRequest {Config = request.Config });
            return Task.FromResult(new StartBotReply
            {
                Reply = new DefaultReply { Message = "est contact", Code = ReplyCode.Succeed }
            });
        }

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


        //return Task.FromResult(new SubscribeLogsReply
        //{ 
        //        //Reply = request.reply
        //        Reply = new DefaultReply { Message = "est contact", Code = ReplyCode.Succeed }
        //});
    }
}

