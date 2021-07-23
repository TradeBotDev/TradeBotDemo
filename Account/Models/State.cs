using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account.Models
{
    public static class State
    {
        // Хранит в себе всех вошедших в аккаунт пользователей, ключем является Id сессии.
        public static Dictionary<string, LoggedAccount> loggedIn;

        // Название файла сохранения.
        public const string LoggedInFilename = "loggedaccounts.state";
    }
}
