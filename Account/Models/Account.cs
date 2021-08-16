using System.Collections.Generic;

namespace AccountGRPC.Models
{
    // Описание таблицы аккаунтов пользователей.
    public class Account
    {
        public int AccountId { get; set; }
        
        public string Email { get; set; }
        
        public string Password { get; set; }

        public List<ExchangeAccess> ExchangeAccesses { get; set; } = new List<ExchangeAccess>();
        
        public LoggedAccount LoggedAccount { get; set; }

        public List<License> Licenses { get; set; } = new List<License>();
    }
}
