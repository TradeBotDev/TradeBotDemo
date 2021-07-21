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

        public override Task<ExchangesBySessionReply> ExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            //var fromAccount = loggedIn[request.SessionId];
            //ExchangesBySessionReply reply = new ExchangesBySessionReply
            //{
            //    Message = "Сообщение",
            //    Result = ActionCode.Successful,
            //};
            //foreach (Models.ExchangeAccess exchange in fromAccount.Exchanges)
            //{
            //    reply.Exchanges.Add(new ExchangeInfo
            //    {
            //        Code = exchange.Code,
            //        Name = exchange.Name,
            //        Token = exchange.Token,
            //        Secret = exchange.Secret
            //    });
            //}
            //return Task.FromResult(reply);
            return base.ExchangesBySession(request, context);
        }
    }
}
