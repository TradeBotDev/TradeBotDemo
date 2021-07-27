using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountGRPC;
using AccountGRPC.Models;
using Xunit;

namespace AccountTests.FileManagementTests
{
    public class ReadFileTests
    {
        // Тестирование чтения файла, который не существует.
        [Fact]
        public void ReadNoExistingTest()
        {
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("not_existing_file.test");
            Assert.Null(file);
        }

        // Тестирование чтения файла, который полностью пуст.
        [Fact]
        public void ReadEmptyFileTest()
        {
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("empty_file.test");
            Assert.Null(file);
        }

        // Тестирование чтения файла, который не содержит в себе информацию.
        [Fact]
        public void ReadNoDataFileTest()
        {
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("not_data_file.test");
            Assert.Null(file);
        }

        // Тестирование чтения файла, который содержит в себе информацию.
        [Fact]
        public void ReadNotEmptyFileTest()
        {
            // Пока пусто
            var file = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>("not_empty_file.test");
            Assert.NotNull(file);
        }
    }
}
