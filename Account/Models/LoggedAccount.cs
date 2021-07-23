namespace Account.Models
{
    // Объект вошедшего аккаунта.
    public class LoggedAccount
    {
        public int AccountId { get; set; }

        // Отвечает за сохранение данных о биржах в базе данных после выхода. True - сохранять, False - уничтожать данные.
        public bool SaveExchangesAfterLogout { get; set; }

        // Этот конструктор необходим для десериализации.
        public LoggedAccount() { }

        public LoggedAccount(Account account, bool saveExchangesAfterLogout)
        {
            this.AccountId = account.AccountId;
            this.SaveExchangesAfterLogout = saveExchangesAfterLogout;
        }
    }
}
