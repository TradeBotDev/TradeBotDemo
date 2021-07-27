using Serilog;
using System.IO;
using System.Text.Json;

namespace AccountGRPC
{
    public static class FileManagement
    {
        // Метод записи объекта любого типа в файл.
        public static async void WriteFile<T>(string filename, T state)
        {
            string serialized = JsonSerializer.Serialize(state);
            await File.WriteAllTextAsync(filename, serialized);
            Log.Information($"Данные записаны в файл {filename}.");
        }

        // Метод чтения любого объекта из файла. Результат записывается в переменную, переданную
        // по ссылке.
        public static T ReadFile<T>(string filename)
        {
            if (File.Exists(filename))
            {
                string file = File.ReadAllText(filename);
                if (!string.IsNullOrEmpty(file))
                {
                    var deserializedFile = JsonSerializer.Deserialize<T>(file);
                    Log.Information($"Файл {filename} успешно прочитан и записан в оперативную память.");
                    return deserializedFile;
                }
            }
            Log.Information($"Ошибка при чтении {filename}: файл не существует или является пустым.");
            return default(T);
        }
    }
}
