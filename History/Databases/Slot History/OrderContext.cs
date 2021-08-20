using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace History.Databases.Slot_History
{
    public class OrderContext : DbContext
    {
        public DbSet<Order> SlotHistory { get; set; }

        public OrderContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));
        }
    }
}
