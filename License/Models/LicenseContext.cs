using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LicenseGRPC.Models
{
    public class LicenseContext : DbContext
    {
        private string connectionString;

        public LicenseContext()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            connectionString = configuration.GetConnectionString("LicenseConnection");
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connectionString);

        public DbSet<License> Licenses { get; set; }
    }
}
