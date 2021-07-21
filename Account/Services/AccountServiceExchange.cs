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
            // Пока ничего особо нет...
            ValidationMessage validationResult = Validate.AddExchangeAccessFields(request);
            if (validationResult.Code != ActionCode.Successful)
            {
                return Task.FromResult(new AddExchangeAccessReply
                {
                    Result = validationResult.Code,
                    Message = validationResult.Message
                });
            }

            using (var database = new Models.AccountContext())
            {
                var account = database.Accounts.Where(account => account.AccountId == loggedIn[request.SessionId].AccountId).First();
                account.ExchangeAccesses.Add(new Models.ExchangeAccess
                {
                    Code = request.Code,
                    Name = request.ExchangeName,
                    Token = request.Token,
                    Secret = request.Secret,
                    Account = account
                });
                database.SaveChanges();
            }
            return Task.FromResult(new AddExchangeAccessReply
            {
                Result = ActionCode.Successful,
                Message = "Успешно"
            });
        }

        public override Task<ExchangesBySessionReply> ExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            // Пока ничего особо не сделано...
            using (Models.AccountContext database = new Models.AccountContext()) {
                var fromAccount = loggedIn[request.SessionId];
                var exchangesFromAccount = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == fromAccount.AccountId);

                ExchangesBySessionReply reply = new ExchangesBySessionReply
                {
                    Message = "Успешно",
                    Result = ActionCode.Successful,
                };

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

                return Task.FromResult(reply);
            }
        }
    }
}
