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
            // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
            // сообщение об ошибке.
            if (!loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(AddExchangeAccessReplies.FailedAddition);

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
                var account = database.Accounts.Where(account => account.AccountId == loggedIn[request.SessionId].AccountId);
                
                // Пока некрасиво сделан ответ :(
                // Проверка на то, добавлен ли токен для той же биржи, что добавляется
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == account.First().AccountId &&
                    exchange.Code == request.Code);

                if (isExists)
                {
                    return Task.FromResult(new AddExchangeAccessReply
                    {
                        Result = ActionCode.ExchangeExists,
                        Message = "Биржа уже существует"
                    });
                }

                account.First().ExchangeAccesses.Add(new Models.ExchangeAccess
                {
                    Code = request.Code,
                    Name = request.ExchangeName,
                    Token = request.Token,
                    Secret = request.Secret,
                    Account = account.First()
                });
                database.SaveChanges();
            }
            return Task.FromResult(AddExchangeAccessReplies.SuccessfulAddition);
        }

        public override Task<AllExchangesBySessionReply> AllExchangesBySession(SessionRequest request, ServerCallContext context)
        {
            if (loggedIn.ContainsKey(request.SessionId))
            {
                using (Models.AccountContext database = new Models.AccountContext())
                {
                    // Получение данных текущей сессии.
                    Models.LoggedAccount fromAccount = loggedIn[request.SessionId];
                    // Поиск всех добавленных бирж по id пользователя текущей сессии.
                    var exchangesFromAccount = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == fromAccount.AccountId);

                    // Ошибка в случае, если биржи не найдены.
                    if (exchangesFromAccount.Count() == 0)
                        return Task.FromResult(AllExchangesBySessionReplies.ExchangesNotFound);

                    // Формирование ответа со всей необходимой информацией о добавленных биржах.
                    var reply = AllExchangesBySessionReplies.SuccessfulGetting(exchangesFromAccount);
                    return Task.FromResult(reply);
                }
            }
            else return Task.FromResult(AllExchangesBySessionReplies.AccountNotFound);
        }

        // Метод, удаляющий данные одной из бирж для текущего пользователя по id записи.
        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            using (var database = new Models.AccountContext())
            {
                // Получение данных биржи для текущего пользователя.
                var exhangeAccess = database.ExchangeAccesses.Where(exchange =>
                    exchange.Account.AccountId == loggedIn[request.SessionId].AccountId &&
                    exchange.ExchangeAccessId == request.ExchangeAccessId);

                // В случае, если такой записи не обнаружено, сервис отвечает ошибкой.
                if (exhangeAccess.Count() == 0)
                    return Task.FromResult(DeleteExchangeAccessReplies.ExchangeNotFound);

                // Если такая запись существует, производится ее удаление.
                database.ExchangeAccesses.Remove(exhangeAccess.First());
                database.SaveChanges();
            }
            return Task.FromResult(DeleteExchangeAccessReplies.SuccessfulDeleting);
        }

        // Пока все сыро и некрасиво, и даже не факт, что работает, ибо я не проверял
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            if (!loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(new ExchangeBySessionReply
                {
                    Result = ActionCode.AccountNotFound,
                    Message = "Произошла ошибка: пользователь не найден",
                    Exchange = null
                });

            using (var database = new Models.AccountContext())
            {
                var exchangeAccesses = database.ExchangeAccesses.Where(exchange => exchange.Code == request.Code &&
                    exchange.Account.AccountId == loggedIn[request.SessionId].AccountId);

                if (exchangeAccesses.Count() == 0)
                {
                    return Task.FromResult(new ExchangeBySessionReply
                    {
                        Result = ActionCode.ExchangeNotFound,
                        Message = "Произошла ошибка: биржа не найдена",
                        Exchange = null
                    });
                }

                var exchangeAccess = exchangeAccesses.First();
                ExchangeBySessionReply reply = new ExchangeBySessionReply
                {
                    Result = ActionCode.Successful,
                    Message = "Успешно",
                    Exchange = new ExchangeAccessInfo
                    {
                        ExchangeAccessId = exchangeAccess.ExchangeAccessId,
                        Code = exchangeAccess.Code,
                        Name = exchangeAccess.Name,
                        Token = exchangeAccess.Token,
                        Secret = exchangeAccess.Secret
                    }
                };
                return Task.FromResult(reply);
            }
        }
    }
}
