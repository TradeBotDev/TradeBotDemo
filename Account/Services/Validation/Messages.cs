using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Validation
{
    public static class Messages
    {
        public static readonly string successfulLogin = "Вход в аккаунт завершен успешно.";
        public static readonly string successfulRegister = "Регистрация завершена успешно.";
        public static readonly string accountNotFound = "Произошла ошибка: пользователь не найден.";
        public static readonly string accountExists = "Произошла ошибка: пользователь уже существует.";
        public static readonly string emptyField = "Произошла ошибка: присутствуют пустые поля. Проверьте правильность введенных данных.";
        public static readonly string passwordMismatch = "Произошла ошибка: введенные пароли не совпадают. Проверьте правильность введенных данных.";
        public static readonly string isNotEmail = "Произошла ошибка: данные не являются электронной почтой. Проверьте правильность введенных данных.";
    }
}
