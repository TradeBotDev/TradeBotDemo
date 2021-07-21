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
        // Метод, добавляющий новую биржу для текущего пользователя.
        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            // Валидация полученных данных. В случае, если валидация не прошла успешно, возвращается сообщение об ошибке.
            ValidationMessage validationResult = Validate.AddExchangeAccessFields(request);
            if (validationResult.Code != ActionCode.Successful)
            {
                return Task.FromResult(new AddExchangeAccessReply
                {
                    Result = validationResult.Code,
                    Message = validationResult.Message
                });
            }

            // Добавление данных из запроса в таблицу базы данных.
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
            return Task.FromResult(ExchangeAccessReplies.SuccessfulAddition);
        }

        public override Task<ExchangesBySessionReply> ExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            using (Models.AccountContext database = new Models.AccountContext()) {
                // Получение данных текущей сессии.
                Models.LoggedAccount fromAccount = loggedIn[request.SessionId];
                // Поиск всех добавленных бирж по id пользователя текущей сессии.
                var exchangesFromAccount = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == fromAccount.AccountId);
                
                // Ошибка в случае, если биржи не найдены.
                if (exchangesFromAccount.Count() == 0)
                    return Task.FromResult(ExchangeAccessReplies.ExchangesNotFound);

                // Формирование ответа со всей необходимой информацией о добавленных биржах.
                var reply = ExchangeAccessReplies.SuccessfulGettingExchangesInfo(exchangesFromAccount);
                return Task.FromResult(reply);
            }
        }

        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            return base.DeleteExchangeAccess(request, context);
        }
    }
}
