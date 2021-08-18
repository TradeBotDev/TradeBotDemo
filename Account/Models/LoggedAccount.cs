using System;

namespace AccountGRPC.Models
{
    // Описание таблицы с вошедшими аккаунтами.
    public class LoggedAccount
    {
        public int LoggedAccountId { get; set; }
        
        public string RefreshToken { get; set; }

        public string LogoutToken { get; set; }

        public string SessionId { get; set; }

        public DateTime LoginDate { get; set; }

        public int AccountId { get; set; }
        
        public Account Account { get; set; }
    }
}
