using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Account.Models
{
    public class AccountContext : DbContext
    {
        public AccountContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=accounts.db");

        public DbSet<Account> Accounts { get; set; }
    }
}
