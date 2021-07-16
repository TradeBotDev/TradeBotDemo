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
        private static ValidationCode ValidateLoginFields(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username)
                || string.IsNullOrEmpty(request.Password))
                return ValidationCode.EmptyField;
            return ValidationCode.Successful;
        }

        // Метод, который проверяет, являются ли ланные для регистрации пустыми
        private static ValidationCode ValidateRegisterFields(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username)
                || string.IsNullOrEmpty(request.Email)
                || string.IsNullOrEmpty(request.Password)
                || string.IsNullOrEmpty(request.VerifyPassword))
                return ValidationCode.EmptyField;

            else if (request.Password != request.VerifyPassword)
                return ValidationCode.PasswordMismatch;

            return ValidationCode.Successful;
        }
    }
}
