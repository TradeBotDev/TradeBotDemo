using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace History.DataBase
{
    public class DataContext : DbContext
    {
        public DbSet<OrderChange> OrderRecords { get; set; }
        public DbSet<BalanceChange> BalanceRecords { get; set; }
        public DataContext()
        {
            Database.EnsureCreated();
        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Order>().HasNoKey();
        //}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=historydb;Username=postgres;Password=password");
        }
    }
}
