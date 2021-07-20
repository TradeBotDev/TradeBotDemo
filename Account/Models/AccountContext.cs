using Microsoft.EntityFrameworkCore;

namespace Account.Models
{
    public class AccountContext : DbContext
    {
        // Создание базы данных, если она отсутствует (к примеру, при первом запуске).
        public AccountContext() => Database.EnsureCreated();
        
        // Указание, что будет использовать SQLite и файл accounts.db для него.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=accounts.db");
        
        // Таблица с данными аккаунтов.
        public DbSet<Account> Accounts { get; set; }
        
        // Таблица с токенами пользователей, соответствующими биржам.
        public DbSet<TradeMarket> TradeMarkets { get; set; }
    }
}
