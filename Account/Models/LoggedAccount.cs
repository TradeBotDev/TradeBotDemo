namespace AccountGRPC.Models
{
    // Объект вошедшего аккаунта.
    public class LoggedAccount
    {
        public int LoggedAccountId { get; set; }
        
        public string SessionId { get; set; }

        public int AccountId { get; set; }
        
        public Account Account { get; set; }
    }
}
