﻿namespace Account.Models
{
    // Описание таблицы аккаунтов пользователей.
    public class Account
    {
        public int AccountId { get; set; }

        public string Email { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }
    }
}