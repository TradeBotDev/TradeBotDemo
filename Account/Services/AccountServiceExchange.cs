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
        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            // Пока пустышка
            return base.AddExchangeAccess(request, context);
        }

        public override Task<ExchangesBySessionReply> ExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            ExchangesBySessionReply reply = new ExchangesBySessionReply
            {
                Message = "Сообщение",
                Result = ActionCode.Successful,
            };

            using (Models.AccountContext database = new Models.AccountContext()) {
                var fromAccount = loggedIn[request.SessionId];
                var exchangesFromAccount = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == fromAccount.AccountId);
                
                foreach (Models.ExchangeAccess exchange in exchangesFromAccount)
                {
                    reply.Exchanges.Add(new ExchangeInfo
                    {
                        Code = exchange.Code,
                        Name = exchange.Name,
                        Token = exchange.Token,
                        Secret = exchange.Secret
                    });
                }
            }
            return Task.FromResult(reply);
        }
    }
}
