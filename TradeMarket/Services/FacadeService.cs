using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Model;

namespace TradeMarket.Services
{
    public class FacadeService : UIInfoService.UIInfoServiceBase
    {
        private SubscriptionService<SubscribeBalanceRequest, SubscribeBalanceReply, Balance, FacadeService> _subscriptionService;

        private ILogger<FacadeService> _logger;

        private SubscribeBalanceReply Convert(Balance balance)
        {
            return null;
        }

        public FacadeService(Subscriber<Balance> subscriber,ILogger<FacadeService> logger)
        {
            _subscriptionService = new(subscriber, logger, Convert);
            _logger = logger;
        }

        public override Task<AuthenticateTokenReply> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AuthenticateTokenReply()
            {
                Reply = new TradeBot.Common.DefaultReply()
                {
                    Code = TradeBot.Common.ReplyCode.Succeed,
                    Message = $"Token {request.Token} autharized"
                }
            });
        }

        public override Task Slots(SlotsRequest request, IServerStreamWriter<SlotsReply> responseStream, ServerCallContext context)
        {
            return Task.FromResult(new SlotsReply()
            {
                SlotName = "XBTUSD"
            }) ;
        }

        public async override Task SubscribeBalance(SubscribeBalanceRequest request, IServerStreamWriter<SubscribeBalanceReply> responseStream, ServerCallContext context)
        {
            await _subscriptionService.Subscribe(request, responseStream, context);
        }
    }
}
