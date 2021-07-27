using System.Collections.Generic;
using AccountGRPC;
using AccountGRPC.Models;
using Xunit;

namespace AccountTests.FileManagementTests
{
    // Класс тестирования чтения данных из файла.
    public class ReadFileTests
    {
        // Тестирование всех вариантов, когда при чтении файла должен возвращаться null.
        [Theory]
        [InlineData("not_existing_file.test")] // Тестирования чтения несуществующего файла.
        [InlineData("empty_file.test")] // Тестирование чтения пустого файла.
        [InlineData("not_data_file.test")] // Тестирование чтения файла без данных (но имеющего символы).
        public void ReadEmptyFile(string filename)
        {
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>(filename);
            Assert.Null(file);
        }

        // Тестирование чтения файла, который содержит в себе информацию.
        [Fact]
        public void ReadNotEmptyFileTest()
        {
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("not_empty_file.test");
            // Ожидается, что в переменной будут содержаться какие-либо данные, полученные из файла.
            Assert.NotNull(file);
        }
    }
}
