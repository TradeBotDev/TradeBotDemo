using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LicenseGRPC.Models
{
    public class LicenseContext : DbContext
    {
        // Строка подключения, которая создается при создании объекта.
        private string connectionString;

        // Создание базы данных, если она отсутствует (к примеру, при первом запуске).
        public LicenseContext()
        {
            // Получение данных из файла appsettings.json.
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Получение строки подкючения из appsettings.json.
            connectionString = configuration.GetConnectionString("LicenseConnection");
            // Создание базы данных, если она еще не была создана.
            Database.EnsureCreated();
        }

        // Указание, что будет использовать SQLite и файл из строки подключения для него (licenses.db).
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connectionString);

        // Таблица с данными о лицензиях.
        public DbSet<License> Licenses { get; set; }
    }
}
