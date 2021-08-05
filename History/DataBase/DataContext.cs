using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace History.DataBase
{
    public class DataContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DataContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=historydb;Username=postgres;Password=password");
        }
    }
}
