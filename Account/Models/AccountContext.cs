using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AccountGRPC.Models
{
    public class AccountContext : DbContext
    {
        // Строка подключения, которая создается при создании объекта.
        private string connectionString;

        // Создание базы данных, если она отсутствует (к примеру, при первом запуске).
        public AccountContext()
        {
            // Получение данных из файла appsettings.json.
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Получение строки подкючения из appsettings.json.
            connectionString = configuration.GetConnectionString("AccountConnection");
            // Создание базы данных, если она еще не была создана.
            Database.EnsureCreated();
        }

        // Указание, что будет использовать SQLite и файл из строки подключения для него (accounts.db).
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connectionString);
        
        // Таблица с данными аккаунтов.
        public DbSet<Account> Accounts { get; set; }

        // Таблица с данными для доступа к биржам.
        public DbSet<ExchangeAccess> ExchangeAccesses { get; set; }

        // Таблица с данными о вошедших аккаунтах.
        public DbSet<LoggedAccount> LoggedAccounts { get; set; }

        public DbSet<License> Licenses { get; set; }
    }
}
