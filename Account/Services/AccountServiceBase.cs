using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account
{
    public partial class AccountService : AcountService.AcountServiceBase
    {
        private readonly ILogger<AccountService> _logger;
        public AccountService(ILogger<AccountService> logger) => _logger = logger;

        Random rnd = new Random();

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var validationResult = ValidateLoginFields(request);

            if (validationResult == ActionCode.EmptyField)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = 0,
                    Message = "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.",
                    Result = validationResult
                });
            }

            return Task.FromResult(new LoginReply
            {
                SessionId = rnd.Next(100, 10000),
                Message = "Вход в аккаунт завершен успешно.",
                Result = ActionCode.Successful
            });
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            //return base.Register(request, context);
            var validationResult = ValidateRegisterFields(request);

            if (validationResult == ActionCode.EmptyField)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult,
                    Message = "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных."
                });
            }

            return Task.FromResult(new RegisterReply
            {
                Result = ActionCode.Successful,
                Message = "Регистрация завершена успешно."
            });
        }

        public override Task<SessionReply> IsValidSession(SessionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SessionReply
            {
                IsValid = true,
                Message = "Текущая сессия валидна"
            });
        }

        public override Task<CurrentUserReply> CurrentUserData(SessionRequest request, ServerCallContext context)
        {
            return base.CurrentUserData(request, context);
        }
    }
}
