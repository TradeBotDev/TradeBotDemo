using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;
using System.Web;

namespace Account
{
    public class AccountService : AcountService.AcountServiceBase
    {
        private readonly ILogger<AccountService> _logger;

        // Хранит в себе всех вошедших в аккаунт пользователей, ключем является Id сессии.
        private Dictionary<string, Models.Account> loggedIn;

        public AccountService(ILogger<AccountService> logger)
        {
            _logger = logger;
            loggedIn = new Dictionary<string, Models.Account>();
        }

        // Метод входа в аккаунт по запросу клиента.
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            // Валидация полей запроса
            (ActionCode, string) validationResult = Validate.LoginFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = "Отсутствует",
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
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
                if (accounts.Count() == 0)
                    return Task.FromResult(new LoginReply
                    {
                        SessionId = "Отсутствует",
                        Result = ActionCode.AccountNotFound,
                        Message = Messages.accountNotFound
                    });

                // В случае наличия зарегистрированного аккаунта с данными из запроса генерируется
                // Id сессии, а также полученный пользователь добавляется в коллекцию с вошедшими
                // пользователями.
                string sessionId = Guid.NewGuid().ToString();
                loggedIn.Add(sessionId, accounts.First());

                // Ответ сервера об успешном входе в аккаунт.
                return Task.FromResult(new LoginReply
                {
                    SessionId = sessionId,
                    Message = Messages.successfulLogin,
                    Result = ActionCode.Successful
                });
            }
        }

        // Метод регистрации аккаунта по запросу клиента. Вход в аккаунт после регистрации НЕ производится!
        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            // Валидация полей запроса
            var validationResult = Validate.RegisterFields(request);

            // В случае, если валидация не прошла успешно (к примеру, присутствуют пустые поля)
            // возвращается сообщение об одной из ошибок в запросе.
            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            // В случае успешной прохождении валидации используется база данных.
            using (var database = new Models.AccountContext()) {
                // Получение всех пользователей с таким же Email-адресом и паролем, как в запросе.
                var accountsWithThisEmail = database.Accounts.Where(accounts => accounts.Email == request.Email);

                // В случае наличия аккаунтов с таким же Email-адресом, как в запросе, возвращается
                // ответ сервера с ошибкой, сообщающей об этом.
                if (accountsWithThisEmail.Count() > 0)
                    return Task.FromResult(new RegisterReply
                    {
                        Result = ActionCode.AccountExists,
                        Message = Messages.accountExists
                    });

                // В случае отсутствия пользователей с тем же Email-адресом, добавление в базу данных
                // нового пользователя с данными из базы данных.
                database.Add(new Models.Account()
                {
                    Email = request.Email,
                    Firstname = request.Firstname,
                    Lastname = request.Lastname,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password
                });
                // Сохранение изменений базы данных.
                database.SaveChanges();

                return Task.FromResult(new RegisterReply
                {
                    Result = ActionCode.Successful,
                    Message = Messages.successfulRegister
                });
            }
        }

        // Метод проверки валидности текущей сессии.
        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            // Проверка на наличие вошедших пользователем с тем же Id сессии, что
            // предоставляется клиентом. Если есть - сессия валидна, нет - невалидна.
            if (loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(new SessionReply
                {
                    IsValid = true,
                    Message = Messages.isValid
                });
            else return Task.FromResult(new SessionReply
            {
                IsValid = false,
                Message = Messages.isNotValid
            });
        }

        // Метод получения информации о текущем пользователе по Id сессии.
        public override Task<CurrentAccountReply> CurrentAccountData(SessionRequest request, ServerCallContext context)
        {
            // Производится проверка на то, является ли текущий пользователь вошедшим (по Id сессии).
            if (!loggedIn.ContainsKey(request.SessionId))
                return Task.FromResult(new CurrentAccountReply
                {
                    Result = ActionCode.AccountNotFound,
                    Message = Messages.accountNotFound,
                    CurrentAccount = new AccountInfo()
                });
            // Если текущий пользователь вошедший, то сервер возвращает данные этого пользователя.
            else return Task.FromResult(new CurrentAccountReply
            {
                Result = ActionCode.Successful,
                Message = Messages.successfulOperation,
                // Информация о пользователе.
                CurrentAccount = new AccountInfo
                {
                    Id = loggedIn[request.SessionId].AccountId,
                    Firstname = loggedIn[request.SessionId].Firstname,
                    Lastname = loggedIn[request.SessionId].Lastname,
                    Email = loggedIn[request.SessionId].Email,
                    PhoneNumber = loggedIn[request.SessionId].PhoneNumber
                }
            });
        }
    }
}
