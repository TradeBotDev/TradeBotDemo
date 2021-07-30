using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Microsoft.EntityFrameworkCore;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using AccountGRPC.Validation.Messages;
using AccountGRPC.Validation;

namespace AccountGRPC
{
    public class ExchangeAccessService : ExchangeAccess.ExchangeAccessBase
    {
        // Добавить биржу для конкретного пользователя.
        public override Task<AddExchangeAccessReply> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            Log.Information($"AddExchangeAccess получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Code - {request.Code}, " +
                $"ExchangeName - {request.ExchangeName}, " +
                $"Token - {request.Token}, " +
                $"Secret - {request.Secret}.");

            using (var database = new Models.AccountContext())
            {
                // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
                // сообщение об ошибке.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return Task.FromResult(AddExchangeAccessReplies.AccountNotFound());

                // Валидация полученных данных. В случае, если валидация не прошла успешно, возвращается сообщение об ошибке.
                ValidationMessage validationResult = Validate.AddExchangeAccessFields(request);
                if (!validationResult.Successful)
                {
                    return Task.FromResult(new AddExchangeAccessReply
                    {
                        Result = ExchangeAccessActionCode.Failed,
                        Message = validationResult.Message
                    });
                }

                // Получение данных о текущем входе (которому соответствует Id сессии) и информации об аккаунте.
                var loginInfo = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account).First();

                // Проверка на то, была ли уже добавлена информация о добавляемой бирже.
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == loginInfo.Account.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если данные доступа к бирже уже существуют, возвращается сообщение об этом.
                if (isExists)
                    return Task.FromResult(AddExchangeAccessReplies.ExchangeAccessExists());

                // Добавление в текущий аккаунт нового доступа к бирже.
                loginInfo.Account.ExchangeAccesses.Add(new Models.ExchangeAccess
                {
                    Code = request.Code,
                    Name = request.ExchangeName,
                    Token = request.Token,
                    Secret = request.Secret,
                    Account = loginInfo.Account
                });
                database.SaveChanges();
            }
            return Task.FromResult(AddExchangeAccessReplies.SuccessfulAddition());
        }

        // Получение данных всех бирж пользователя.
        public override Task<AllExchangesBySessionReply> AllExchangesBySession(AllExchangesBySessionRequest request, ServerCallContext context)
        {
            Log.Information($"AllExchangesBySession получил запрос: SessionId - {request.SessionId}.");
            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (login.Count() == 0)
                    return Task.FromResult(AllExchangesBySessionReplies.AccountNotFound());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Account.AccountId == login.First().Account.AccountId);

                // Ошибка в случае, если биржи не найдены.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(AllExchangesBySessionReplies.ExchangesNotFound());

                // Формирование ответа со всей необходимой информацией о добавленных биржах.
                var reply = AllExchangesBySessionReplies.SuccessfulGetting(exchangeAccesses);
                return Task.FromResult(reply);
            }
        }

        // Удалить биржу из аккаунта пользователя.
        public override Task<DeleteExchangeAccessReply> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            Log.Information($"DeleteExchangeAccess получил запрос: SessionId - {request.SessionId}, Code - {request.Code}.");

            using (var database = new Models.AccountContext())
            {
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return Task.FromResult(DeleteExchangeAccessReplies.AccountNotFound());

                Models.LoggedAccount fromAccount = database.LoggedAccounts.Where(login => login.SessionId == request.SessionId).First();

                // Получение данных биржи для текущего пользователя и биржи с конкретным кодом.
                var exсhangeAccess = database.ExchangeAccesses.Where(exchange =>
                    exchange.Account.AccountId == fromAccount.Account.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если такой записи не обнаружено, сервис отвечает ошибкой.
                if (exсhangeAccess.Count() == 0)
                    return Task.FromResult(DeleteExchangeAccessReplies.ExchangeNotFound());

                // Если такая запись существует, производится ее удаление.
                database.ExchangeAccesses.Remove(exсhangeAccess.First());
                database.SaveChanges();
            }
            return Task.FromResult(DeleteExchangeAccessReplies.SuccessfulDeleting());
        }

        // Получение данных конкретной биржи пользователя.
        public override Task<ExchangeBySessionReply> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            Log.Information($"ExchangeBySession получил запрос: SessionId - {request.SessionId}, Code - {request.Code}.");
            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (login.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.AccountNotFound());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Code == request.Code &&
                        exchange.Account.AccountId == login.First().Account.AccountId);

                // Если такой биржи не было найдено, возвращается сообщение об ошибке.
                if (exchangeAccesses.Count() == 0)
                    return Task.FromResult(ExchangeBySessionReplies.ExchangeNotFound());

                // В случае успешного получения доступа к бирже она возвращается в качестве ответа.
                var exchangeAccess = exchangeAccesses.First();
                return Task.FromResult(ExchangeBySessionReplies.SuccessfulGettingExchangeAccess(exchangeAccess));
            }
        }
    }
}
