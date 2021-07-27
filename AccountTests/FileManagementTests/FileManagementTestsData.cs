using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountTests.FileManagementTests
{
    // Класс для полей, необходимых для тестирования чтения и записи файлов.
    public class FileManagementTestsData
    {
        // Дубликат класса с вошедшими аккаунтами.
        public Dictionary<string, AccountGRPC.Models.LoggedAccount> loggedIn = new();
    }
}
