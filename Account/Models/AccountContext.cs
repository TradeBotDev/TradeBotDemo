﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Подключение к СУБД в Docker:
// docker run -p 5432:5432 -d -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -e POSTGRES_DB=postgresdb postgres
// Для применения миграций в Visual Studio:
// Update-Database
// Для применения миграций из командной строки (из папки проекта):
// dotnet ef database update

namespace AccountGRPC.Models
{
    public class AccountContext : DbContext
    {
        // Строка подключения, которая создается при создании объекта.
        private string connectionString;

        // Создание базы данных, если она отсутствует (к примеру, при первом запуске).
        public AccountContext()
        {
            connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
            Database.EnsureCreated();
        }

        // Указание, что будет использовать PostgreSQL и файл из строки подключения для него.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(connectionString);

        // Таблица с данными аккаунтов.
        public DbSet<Account> Accounts { get; set; }

        // Таблица с данными для доступа к биржам.
        public DbSet<ExchangeAccess> ExchangeAccesses { get; set; }

        // Таблица с данными о вошедших аккаунтах.
        public DbSet<LoggedAccount> LoggedAccounts { get; set; }

        public DbSet<License> Licenses { get; set; }
    }
}