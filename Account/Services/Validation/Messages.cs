﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Validation
{
    public static class Messages
    {
        public static readonly string valid = "Валидация прошла успешно.";
        public static readonly string userNotFound = "Произошла ошибка: пользователь не найден.";
        public static readonly string userExists = "Произошла ошибка: пользователь уже существует.";
        public static readonly string emptyField = "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";
        public static readonly string passwordMismatch = "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";
        public static readonly string isNotEmail = "Произошла ошибка: данные не являются электронной почтой. Проверьте правильность введенных данных.";
    }
}
