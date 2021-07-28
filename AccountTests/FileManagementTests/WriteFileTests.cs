using System.Collections.Generic;
using AccountGRPC;
using AccountGRPC.Models;
using Xunit;

namespace AccountTests.FileManagementTests
{
    [Collection("AccountTests")]
    public class WriteFileTests : FileManagementTestsData
    {
        // Тестирование записи объекта, содержащего в себе данные.
        [Fact]
        public void WriteNotNullTest()
        {
            // Добавление случайных записей в список вошедших пользователей.
            for (int i = 0; i < 10; i++)
            {
                loggedIn.Add($"random_session_{random.Next(1, 1000)}", new LoggedAccount
                {
                    AccountId = random.Next(1, 100),
                    SaveExchangesAfterLogout = false
                });
            }
            // Запись в переменные результата записи файла, а зачем его чтения.
            bool isWrited = FileManagement.WriteFile("not_null_writed_file.test", loggedIn);
            var writed_file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("not_null_writed_file.test");

            // Ожидается, что запись будет произведена корректно, а объект будет содержать в себе данные.
            Assert.True(isWrited);
            Assert.NotNull(writed_file);
        }

        // Тестирование записи пустого объекта, который не содержит в себе данные.
        [Fact]
        public void WriteNullTest()
        {
            // Происходит попытка записать в файл null, а затем прочитать его.
            bool isWrited = FileManagement.WriteFile<object>("null_writed_file.test", null);
            var writed_file = FileManagement.ReadFile<object>("null_writed_file.test");

            // Ожидается, что запись файла не будет произведена, а файл будет null.
            Assert.False(isWrited);
            Assert.Null(writed_file);
        }
    }
}
