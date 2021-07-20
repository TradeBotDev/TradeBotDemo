using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation;

namespace Account
{
    public partial class AccountService : TradeBot.Account.AccountService.v1.Account.AccountBase
    {
        private readonly ILogger<AccountService> _logger;

        // Хранит в себе всех вошедших в аккаунт пользователей, ключем является Id сессии.
        private static Dictionary<string, Models.Account> loggedIn;

        private static string loggedInFilename = "loggedaccounts.state";

        public AccountService(ILogger<AccountService> logger)
        {
            // В случае, если чтение файла не прошло успешно, а loggedIn является пустым, для него
            // выделяется память.
            if (!FileManagement.ReadState(loggedInFilename, ref loggedIn) && loggedIn == null)
                loggedIn = new Dictionary<string, Models.Account>();
            _logger = logger;
        }

        // Метод выхода из аккаунта
        public override Task<LogoutReply> Logout(SessionRequest request, ServerCallContext context)
        {
            // В случае успешного удаления аккаунта из списка, в которые пользователь зашел,
            // сервер отвечает, что выход был успешно завершен.
            if (loggedIn.Remove(request.SessionId))
            {
                FileManagement.WriteState(loggedInFilename, loggedIn);
                return Task.FromResult(new LogoutReply
                {
                    Result = ActionCode.Successful,
                    Message = Messages.successfulLogout
                });
            }
            // В случае, если аккаунт не был удален (по причине отсутствия) появляется сообщение об ошибке.
            else return Task.FromResult(new LogoutReply
            {
                Result = ActionCode.AccountNotFound,
                Message = Messages.failedLogout
            });
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
            {
                return Task.FromResult(new CurrentAccountReply
                {
                    Result = ActionCode.AccountNotFound,
                    Message = Messages.accountNotFound,
                    CurrentAccount = new AccountInfo()
                });
            }
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
