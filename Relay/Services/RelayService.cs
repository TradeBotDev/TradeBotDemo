using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Relay.Facade;
using TradeBot.Common;

namespace Relay
{
    class RelayService : SendToRelay.SendToRelayBase
    {
        private readonly ILogger<RelayService> _logger;
        public RelayService(ILogger<RelayService> logger) => _logger = logger;

        //public override Task<RelayService> StartBot(StartBotRequest request, ServerCallContext context)
        //{
        //    DefaultReply reply = new DefaultReply
        //    {
        //        Code = ReplyCode.Succeed,
        //        Message = "Бот запущен успешно"
        //    };

        //    StartBotReply result = new StartBotReply
        //    {
        //        Reply = reply
        //    };

            
        //}
    }
}
