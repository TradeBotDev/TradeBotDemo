using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Account
{
    public static class FileManagement
    {
        // Метод записи объекта любого типа в файл.
        public static async void WriteState<T>(string filename, T state)
        {
            string serialized = JsonSerializer.Serialize(state);
            await File.WriteAllTextAsync(filename, serialized);
        }

        // Метод чтения любого объекта из файла. Результат записывается в переменную, переданную
        // по ссылке.
        public static bool ReadState<T>(string filename, ref T state)
        {
            if (File.Exists(filename) && state != null)
            {
                string file = File.ReadAllText(filename);
                state = JsonSerializer.Deserialize<T>(file);
                return true;
            }
            else return false;
        }
    }
}
