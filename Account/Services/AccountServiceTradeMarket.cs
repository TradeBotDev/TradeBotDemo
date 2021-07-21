using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;
using Account.Validation.Messages;
using Account.AccountMessages;

namespace Account
{
    public partial class AccountService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        public override Task<AddTokenReply> AddToken(AddTokenRequest request, ServerCallContext context)
        {
            return base.AddToken(request, context);
        }

        public override Task<TMsBySessionReply> TMsBySession(SessionRequest request, ServerCallContext context)
        {
            var fromAccount = loggedIn[request.SessionId];

            TMsBySessionReply reply = new TMsBySessionReply
            {
                Message = "Сообщение",
                Result = ActionCode.Successful,
            };

            foreach (Models.TradeMarketAccess tradeMarket in fromAccount.TradeMarkets)
            {
                reply.TradeMarkets.Add(new TradeMarketInfo
                {
                    Name = tradeMarket.Name,
                    Token = tradeMarket.Token,
                    Secret = tradeMarket.Secret
                });
            }
            return Task.FromResult(reply);
        }
    }
}
