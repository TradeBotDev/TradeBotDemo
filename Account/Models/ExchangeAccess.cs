using TradeBot.Account.AccountService.v1;

namespace AccountGRPC.Models
{
    // Описание таблицы с информацией о доступе к бирже пользователя.
    public class ExchangeAccess
    {
        public int ExchangeAccessId { get; set; }

        public ExchangeAccessCode Code { get; set; }
        
        public string Name { get; set; }
        
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public string LogoutToken { get; set; }
        
        public string Secret { get; set; }

        public Account Account { get; set; }
    }
}
