using System;
using System.Collections.Generic;

namespace AccountTests.FileManagementTests
{
    // Класс для полей, необходимых для тестирования чтения и записи файлов.
    public abstract class FileManagementTestsData
    {
        public Random random = new();

        // Дубликат класса с вошедшими аккаунтами.
        public Dictionary<string, AccountGRPC.Models.LoggedAccount> loggedIn = new();
    }
}
