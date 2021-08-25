using System;
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
    public class AccountService : Account.AccountBase
    {
        // Логгирование.
        protected readonly ILogger logger = Log.ForContext("Where", "AccountService");

        // Метод входа в аккаунт по запросу клиента.
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"Email - {request.Email}, " +
                $"Password - {request.Password}.", GetType().Name, "Login");

            // Валидация полей запроса
            var validationResult = Validate.LoginFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (!validationResult.Successful)
            {
                return await Task.FromResult(new LoginResponse
                {
                    SessionId = "none",
                    Result = AccountActionCode.Failed,
                    Message = validationResult.Message
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext())
            {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accounts = database.Accounts.Where(accounts => accounts.Email == request.Email &&
                    accounts.Password == request.Password);

                // Проверка на наличие зарегистрированных аккаунтов с данными из запроса, и в
                // случае их отсутствия отправляет ответ с сообщением об ошибке.
                if (!accounts.Any())
                    return await Task.FromResult(LoginReplies.AccountNotFound());

                // Проверка на то, есть ли сессия с пользователем, который пытается войти в аккаунт, и
                // в случае, если он вошел, возвращается его Id сессии
                var existingLogin = database.LoggedAccounts.Where(account => account.AccountId == accounts.First().AccountId);
                if (existingLogin.Count() > 0)
                {
                    string newSessionId = Guid.NewGuid().ToString();
                    existingLogin.First().SessionId = newSessionId;
                    existingLogin.First().LoginDate = DateTime.Now;
                    database.SaveChanges();

                    return await Task.FromResult(LoginReplies.AlreadySignedIn(newSessionId));
                }

                // В случае наличия зарегистрированного аккаунта с данными из запроса генерируется
                // Id сессии, а также полученный пользователь добавляется в таблицу с вошедшими
                // пользователями.
                string sessionId = Guid.NewGuid().ToString();
                var loggedAccount = new Models.LoggedAccount
                {
                    SessionId = sessionId,
                    LoginDate = DateTime.Now,
                    Account = accounts.First()
                };
                // Добавление в таблицу информации о новом входе в аккаунт.
                database.LoggedAccounts.Add(loggedAccount);
                database.SaveChanges();

                // Ответ сервера об успешном входе в аккаунт.
                return await Task.FromResult(LoginReplies.SuccessfulLogin(sessionId));
            }
        }

        // Метод выхода из аккаунта
        public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}, " +
                $"SaveExchangeAccesses - {request.SaveExchangeAccesses}.", GetType().Name, "Logout");

            using (var database = new Models.AccountContext())
            {
                // В случае, если аккаунт не был найден среди вошедших, появляется сообщение об ошибке.
                if (!database.LoggedAccounts.Any(login => login.SessionId == request.SessionId))
                    return await Task.FromResult(LogoutReplies.AccountNotFound());

                // Получение информации о входе вместе с данными о пользователе.
                var loginInfo = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account).First();

                // Если отключено сохранение всех данных о биржах пользователя после выхода из аккаунта, происходит
                // их удаление из базы данных (только связанные с этим пользователем).
                if (!request.SaveExchangeAccesses)
                {
                    var exchanges = database.ExchangeAccesses.Where(exchange => exchange.Account.AccountId == loginInfo.Account.AccountId);
                    foreach (Models.ExchangeAccess exchange in exchanges)
                        database.ExchangeAccesses.Remove(exchange);
                }
                
                // Удаление аккаунта из списка вошедших.
                database.LoggedAccounts.Remove(loginInfo);
                database.SaveChanges();
            }
            // В случае успешного удаления аккаунта из списка, в который пользователь зашел,
            // сервер отвечает, что выход был успешно завершен.
            return await Task.FromResult(LogoutReplies.SuccessfulLogout());
        }

        // Метод регистрации аккаунта по запросу клиента.
        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"Email - {request.Email}, " +
                $"Password - {request.Password}, " +
                $"VerifyPassword - {request.VerifyPassword}.", GetType().Name, "Register");

            // Валидация полей запроса
            var validationResult = Validate.RegisterFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (!validationResult.Successful)
            {
                return await Task.FromResult(new RegisterResponse
                {
                    Result = AccountActionCode.Failed,
                    Message = validationResult.Message
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using var database = new Models.AccountContext();
            // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
            var accountsWithThisEmail = database.Accounts.Where(accounts => accounts.Email == request.Email);

            // В случае наличия аккаунтов с таким же Email-адресом, как в запросе, возвращается
            // ответ сервера с ошибкой, сообщающей об этом.
            if (accountsWithThisEmail.Any())
                return await Task.FromResult(RegisterReplies.AccountExists());

            // В случае отсутствия пользователей с тем же Email-адресом, добавление в базу данных
            // нового пользователя с данными из базы данных.
            database.Add(new Models.Account()
            {
                Email = request.Email,
                Password = request.Password
            });
            // Сохранение изменений базы данных и возвращение ответа.
            database.SaveChanges();
            return await Task.FromResult(RegisterReplies.SuccessfulRegister());
        }

        // Метод проверки валидности текущей сессии.
        public override async Task<IsValidSessionResponse> IsValidSession(IsValidSessionRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}.", GetType().Name, "IsValidSession");

            using (var database = new Models.AccountContext())
            {
                // Проверка на наличие вошедших пользователем с тем же Id сессии, что
                // предоставляется клиентом. Если есть и время сессии не вышло - сессия валидна.
                var account = database.LoggedAccounts.Where(account => account.SessionId == request.SessionId);

                if (account.Count() >= 0 && !LoggedAccountsManagement.TimeOutAction(request.SessionId))
                    return await Task.FromResult(IsValidSessionReplies.IsValid());
                // Если нет - сессия невалидна.
                else return await Task.FromResult(IsValidSessionReplies.IsNotValid());
            }
        }

        // Метод получения информации о текущем пользователе по Id сессии.
        public override async Task<AccountDataResponse> AccountData(AccountDataRequest request, ServerCallContext context)
        {
            logger.Information("{@Service} - {@Method} получил запрос: " +
                $"SessionId - {request.SessionId}.", GetType().Name, "AccountData");

            using var database = new Models.AccountContext();
            var checkAccount = database.LoggedAccounts.Where(account => account.SessionId == request.SessionId);

            // Производится проверка на то, является ли текущий пользователь вошедшим (по Id сессии).
            if (!checkAccount.Any())
                return await Task.FromResult(AccountDataReplies.AccountNotFound());
            else if (LoggedAccountsManagement.TimeOutAction(request.SessionId))
                return await Task.FromResult(AccountDataReplies.TimePassed());

            // Если текущий пользователь вошедший, то сервер возвращает данные этого пользователя.
            else
            {
                // Получение данных вошедшего пользователя.
                var login = database.LoggedAccounts
                    .Where(login => login.SessionId == request.SessionId)
                    .Include(account => account.Account)
                    .Include(exchange => exchange.Account.ExchangeAccesses).First();

                // Формирование ответа.
                var reply = AccountDataReplies.SuccessfulGettingAccountData(login);
                return await Task.FromResult(reply);
            }
        }
    }
}
