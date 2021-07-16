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
    public class AccountService : AcountService.AcountServiceBase
    {
        private readonly ILogger<AccountService> _logger;
        public AccountService(ILogger<AccountService> logger) => _logger = logger;

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var validationResult = Validate.LoginFields(request);

            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new LoginReply
                {
                    SessionId = 0,
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
                });
            }

            return Task.FromResult(new LoginReply
            {
                SessionId = 1,
                Message = "Вход в аккаунт завершен успешно.",
                Result = ActionCode.Successful
            });
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            //return base.Register(request, context);
            var validationResult = Validate.RegisterFields(request);

            if (validationResult.Item1 != ActionCode.Successful)
            {
                return Task.FromResult(new RegisterReply
                {
                    Result = validationResult.Item1,
                    Message = validationResult.Item2
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
