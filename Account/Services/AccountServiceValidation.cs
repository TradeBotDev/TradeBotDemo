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
        // Метод, который проверяет, являются ли ланные для входа пустыми
        private static ActionCode ValidateLoginFields(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username)
                || string.IsNullOrEmpty(request.Password))
                return ActionCode.EmptyField;
            return ActionCode.Successful;
        }

        // Метод, который проверяет, являются ли ланные для регистрации пустыми
        private static ActionCode ValidateRegisterFields(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username)
                || string.IsNullOrEmpty(request.Email)
                || string.IsNullOrEmpty(request.Password)
                || string.IsNullOrEmpty(request.VerifyPassword))
                return ActionCode.EmptyField;

            else if (request.Password != request.VerifyPassword)
                return ActionCode.PasswordMismatch;

            return ActionCode.Successful;
        }
    }
}
