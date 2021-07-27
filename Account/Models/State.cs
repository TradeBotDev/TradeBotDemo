using System.Collections.Generic;

namespace AccountGRPC.Models
{
    public static class State
    {
        // Хранит в себе всех вошедших в аккаунт пользователей, ключем является Id сессии.
        public static Dictionary<string, LoggedAccount> loggedIn;

        // Название файла сохранения.
        public const string LoggedInFilename = "loggedaccounts.state";
    }
}
