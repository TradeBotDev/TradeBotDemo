using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;

namespace Account.Validation
{
    public class Validate
    {
        // Метод, который проверяет, являются ли ланные для входа пустыми
        public static (ActionCode, string) LoginFields(LoginRequest request)
        {
            if (IsEmpty(request.Email, request.Password))
                return (ActionCode.EmptyField, Messages.emptyField);

            return (ActionCode.Successful, Messages.successfulValidation);
        }

        // Метод, который проверяет, являются ли ланные для регистрации пустыми
        public static (ActionCode, string) RegisterFields(RegisterRequest request)
        {
            if (IsEmpty(request.Email, request.Firstname, request.Lastname, request.Password, request.VerifyPassword))
                return (ActionCode.EmptyField, Messages.emptyField);

            else if (request.Password != request.VerifyPassword)
                return (ActionCode.PasswordMismatch, Messages.passwordMismatch);

            else if (!request.Email.Contains('@'))
                return (ActionCode.IsNotEmail, Messages.isNotEmail);

            return (ActionCode.Successful, Messages.successfulValidation);
        }

        //Метод, который пробегается по всем предоставленным строкам и делает вывод, являются ли они пустыми
        private static bool IsEmpty(params string[] fields)
        {
            foreach (string field in fields)
                if (string.IsNullOrEmpty(field))
                    return true;
            return false;
        }
    }
}
