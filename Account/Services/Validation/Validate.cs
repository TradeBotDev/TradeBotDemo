﻿using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Account.AccountService.v1;
using Account.Validation.Messages;

namespace Account.Validation
{
    public class Validate
    {
        // Метод, который проверяет, являются ли поля для входа пустыми.
        public static ValidationMessage LoginFields(LoginRequest request)
        {
            // В случае, если они являются пустыми, возвращает сообщение об ошибке.
            if (IsEmpty(request.Email, request.Password))
                return new EmptyFieldMessage();
            // Иначе возвращает сообщение об успешности операции.
            return new SuccessfulValidationMessage();
        }

        // Метод, который проверяет, являются ли ланные для регистрации пустыми и нет ли в них ошибок.
        public static ValidationMessage RegisterFields(RegisterRequest request)
        {
            // В случае, если хоть одно поле пустое, возвращает сообщение о наличии пустых полей.
            if (IsEmpty(request.Email, request.Firstname, request.Lastname, request.PhoneNumber, request.Password, request.VerifyPassword))
                return new EmptyFieldMessage();

            // В случае, если пароли не совпадают, возвращает сообщение о несоответствии паролей.
            else if (request.Password != request.VerifyPassword)
                return new PasswordMismatchMessage();

            // В случае, если при записи адреса электронной почты допущены ошибки, возвращает сообщение о том,
            // что данные не являются адресом электронной почтой.
            else if (!IsEmail(request.Email))
                return new IsNotEmailMessage();
            
            // В случае отсутствия ошибок возвращает сообщение об успешности валидации.
            return new SuccessfulValidationMessage();
        }

        //Метод, который пробегается по всем предоставленным строкам и делает вывод, являются ли они пустыми.
        private static bool IsEmpty(params string[] fields)
        {
            foreach (string field in fields)
                if (string.IsNullOrEmpty(field))
                    return true;
            return false;
        }

        // Метод, проверяющий Email-адрес на корректность.
        private static bool IsEmail(string email)
        {
            if (email.Contains('@')) return true;
            else return false;
        }
    }
}
