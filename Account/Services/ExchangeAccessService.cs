using System.Linq;
using System.Threading.Tasks;

using Grpc.Core;
using Serilog;
using Microsoft.EntityFrameworkCore;

using TradeBot.Account.AccountService.v1;
using AccountGRPC.AccountMessages;
using AccountGRPC.Validation;

namespace AccountGRPC
{
    public class ExchangeAccessService : ExchangeAccess.ExchangeAccessBase
    {
        // Логгирование.
        protected readonly ILogger logger = Log.ForContext("Where", "AccountService");

        // Добавить биржу для конкретного пользователя.
        public override async Task<AddExchangeAccessResponse> AddExchangeAccess(AddExchangeAccessRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Code - {request.Code}, " +
                $"ExchangeName - {request.ExchangeName}, " +
                $"Token - {request.Token}, " +
                $"Secret - {request.Secret}.", GetType().Name, "AddExchangeAccess");

            using (var database = new Models.AccountContext())
            {
                // Проверка на существование входа в аккаунт. Если аккаунт среди вошедших не найден, отправляется
                // сообщение об ошибке.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return await Task.FromResult(AddExchangeAccessReplies.AccountNotFound());
                else if (LoggedAccountsManagement.TimeOutAction(request.SessionId))
                    return await Task.FromResult(AddExchangeAccessReplies.TimePassed());

                // Валидация полученных данных. В случае, если валидация не прошла успешно, возвращается сообщение об ошибке.
                var validationResult = Validate.AddExchangeAccessFields(request);
                if (!validationResult.Successful)
                {
                    return await Task.FromResult(new AddExchangeAccessResponse
                    {
                        Result = ExchangeAccessActionCode.Failed,
                        Message = validationResult.Message
                    });
                }

                // Получение данных о текущем входе (которому соответствует Id сессии) и информации об аккаунте.
                var loginInfo = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account)
                    .Include(account => account.Account.Licenses).First();

                // Проверка на то, обладает ли пользователь лицензией. Если не обладает, возвращается сообщиние об ошибке.
                if (loginInfo.Account.Licenses.Count() == 0)
                    return await Task.FromResult(AddExchangeAccessReplies.LicenseNotFound());

                // Проверка на то, была ли уже добавлена информация о добавляемой бирже.
                bool isExists = database.ExchangeAccesses.Any(exchange =>
                    exchange.Account.AccountId == loginInfo.Account.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если данные доступа к бирже уже существуют, возвращается сообщение об этом.
                if (isExists)
                    return await Task.FromResult(AddExchangeAccessReplies.ExchangeAccessExists());

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
            return await Task.FromResult(AddExchangeAccessReplies.SuccessfulAddition());
        }

        // Получение данных всех бирж пользователя.
        public override async Task<AllExchangesBySessionResponse> AllExchangesBySession(AllExchangesBySessionRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}.", GetType().Name, "AllExchangesBySession");

            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (!login.Any())
                    return await Task.FromResult(AllExchangesBySessionReplies.AccountNotFound());
                else if (LoggedAccountsManagement.TimeOutAction(request.SessionId))
                    return await Task.FromResult(AllExchangesBySessionReplies.TimePassed());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Account.AccountId == login.First().Account.AccountId);

                // Ошибка в случае, если биржи не найдены.
                if (!exchangeAccesses.Any())
                    return await Task.FromResult(AllExchangesBySessionReplies.ExchangesNotFound());

                // Формирование ответа со всей необходимой информацией о добавленных биржах.
                var reply = AllExchangesBySessionReplies.SuccessfulGetting(exchangeAccesses);
                return await Task.FromResult(reply);
            }
        }

        // Удалить биржу из аккаунта пользователя.
        public override async Task<DeleteExchangeAccessResponse> DeleteExchangeAccess(DeleteExchangeAccessRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Code - {request.Code}.", GetType().Name, "DeleteExchangeAccess");

            using (var database = new Models.AccountContext())
            {
                // Если среди вошедших аккаунтов не был найден нужный, отправляется сообщение о том, что аккаунт
                // не был найден.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return await Task.FromResult(DeleteExchangeAccessReplies.AccountNotFound());
                else if (LoggedAccountsManagement.TimeOutAction(request.SessionId))
                    return await Task.FromResult(DeleteExchangeAccessReplies.TimePassed());


                // Если аккаунт был найден, производится получение его данных по Id сессии.
                var fromAccount = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId).First();

                // Получение нужной информации о доступе к бирже, соответствующей полученному аккаунту
                // и выбранной бирже.
                var exchangeAccess = database.ExchangeAccesses
                    .Where(exchange => exchange.Account.AccountId == fromAccount.AccountId &&
                    exchange.Code == request.Code);

                // В случае, если такой записи не обнаружено, сервис отвечает ошибкой.
                if (exchangeAccess.Count() == 0)
                    return await Task.FromResult(DeleteExchangeAccessReplies.ExchangeNotFound());

                // Если такая запись существует, производится ее удаление.
                database.ExchangeAccesses.Remove(exchangeAccess.First());
                database.SaveChanges();
            }
            return await Task.FromResult(DeleteExchangeAccessReplies.SuccessfulDeleting());
        }

        // Получение данных конкретной биржи пользователя.
        public override async Task<ExchangeBySessionResponse> ExchangeBySession(ExchangeBySessionRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"Code - {request.Code}.", GetType().Name, "ExchangeBySession");

            using (var database = new Models.AccountContext())
            {
                // Получение данных о входе по текущему Id сессии.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account);

                // В случае, если аккаунт не найден среди вошедших, возвращается сообщение об ошибке.
                if (!login.Any())
                    return await Task.FromResult(ExchangeBySessionReplies.AccountNotFound());
                else if (LoggedAccountsManagement.TimeOutAction(request.SessionId))
                    return await Task.FromResult(ExchangeBySessionReplies.TimePassed());

                // Поиск информации о доступе пользователя к бирже.
                var exchangeAccesses = database.ExchangeAccesses
                    .Include(account => account.Account)
                    .Where(exchange => exchange.Code == request.Code &&
                        exchange.Account.AccountId == login.First().Account.AccountId);

                // Если такой биржи не было найдено, возвращается сообщение об ошибке.
                if (!exchangeAccesses.Any())
                    return await Task.FromResult(ExchangeBySessionReplies.ExchangeNotFound());

                // В случае успешного получения доступа к бирже она возвращается в качестве ответа.
                var exchangeAccess = exchangeAccesses.First();
                return await Task.FromResult(ExchangeBySessionReplies.SuccessfulGettingExchangeAccess(exchangeAccess));
            }
        }
    }
}
