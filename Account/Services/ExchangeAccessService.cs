using Account.AccountMessages;
using Account.Validation;
using Account.Validation.Messages;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Models;
using Microsoft.Extensions.Logging;

namespace Account
{
    public class ExchangeAccessService : TradeBot.Account.AccountService.v1.ExchangeAccess.ExchangeAccessBase
    {
        private readonly ILogger<ExchangeAccessService> _logger;

        public ExchangeAccessService(ILogger<ExchangeAccessService> logger) => _logger = logger;

        // Метод, добавляющий новую биржу для текущего пользователя.
        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
            // сообщение об ошибке.
            if (!State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(AddExchangeAccessReplies.AccountNotFound);

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
            using (var database = new AccountContext())
            {
                var account = database.Accounts.Where(account => account.AccountId == State.loggedIn[request.SessionId].AccountId);

                // Проверка на то, добавлен ли токен для той же биржи, что добавляется.
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == account.First().AccountId &&
                    exchange.Code == request.Code);

                // В случае, если данные доступа к бирже уже существуют, возвращается сообщение об этом.
                if (isExists)
                    return Task.FromResult(AddExchangeAccessReplies.ExchangeAccessExists);

                // Добавление в текущий аккаунт нового доступа к бирже.
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
            if (State.loggedIn.ContainsKey(request.SessionId))
            {
                using (var database = new AccountContext())
                {
                    // Получение данных текущей сессии.
                    LoggedAccount fromAccount = State.loggedIn[request.SessionId];
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
            using (var database = new AccountContext())
            {
                // Получение данных биржи для текущего пользователя и биржи с конкретным кодом.
                var exhangeAccess = database.ExchangeAccesses.Where(exchange =>
                    exchange.Account.AccountId == State.loggedIn[request.SessionId].AccountId &&
                    exchange.Code == request.Code);

                // В случае, если такой записи не обнаружено, сервис отвечает ошибкой.
                if (exhangeAccess.Count() == 0)
                    return Task.FromResult(DeleteExchangeAccessReplies.ExchangeNotFound);

                // Если такая запись существует, производится ее удаление.
                database.ExchangeAccesses.Remove(exhangeAccess.First());
                database.SaveChanges();
            }
            return Task.FromResult(DeleteExchangeAccessReplies.SuccessfulDeleting);
        }

        // Получение данных конкретной биржи пользователя.
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
            if (!State.loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(ExchangeBySessionReplies.AccountNotFound);

            using (var database = new AccountContext())
            {
                // Поиск данных конкретной биржи пользователя.
                var exchangeAccesses = database.ExchangeAccesses.Where(exchange => exchange.Code == request.Code &&
                    exchange.Account.AccountId == State.loggedIn[request.SessionId].AccountId);
                // Если такой биржи не было найдено, возвращается сообшение об ошибке.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.ExchangeNotFound);
                // В случае успешного получения доступа к бирже она возвращается в качестве ответа.
                var exchangeAccess = exchangeAccesses.First();
                return Task.FromResult(ExchangeBySessionReplies.SuccessfulGettingExchangeAccess(exchangeAccess));
            }
        }
    }
}
