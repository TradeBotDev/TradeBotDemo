﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Validation
{
    public static class Messages
    {
        public const string successfulOperation = "Операция завершена успешно.";
        public const string successfulValidation = "Валидация завершена успешно.";
        public const string successfulLogin = "Вход в аккаунт завершен успешно.";
        public const string successfulRegister = "Регистрация завершена успешно.";
        public const string accountNotFound = "Произошла ошибка: пользователь не найден.";
        public const string accountExists = "Произошла ошибка: пользователь уже существует.";
        public const string emptyField = "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";
        public const string passwordMismatch = "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";
        public const string isNotEmail = "Произошла ошибка: данные не являются электронной почтой. Проверьте правильность введенных данных.";
        public const string isValid = "Операция валидна.";
        public const string isNotValid = "Операция не валидна.";
    }
}
