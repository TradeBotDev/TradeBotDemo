using Serilog;
using System.IO;
using System.Text.Json;

namespace Account
{
    public static class FileManagement
    {
        // Метод записи объекта любого типа в файл.
        public static async void WriteFile<T>(string filename, T state)
        {
            Log.Debug($"WriteFile - файл {filename} записан в state с типом {state.GetType()}.");
            string serialized = JsonSerializer.Serialize(state);
            await File.WriteAllTextAsync(filename, serialized);
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
                    Log.Debug("ReadFile - файл прочитан.");
                    return deserializedFile;
                }
                Log.Debug("ReadFile - файл пуст.");
            }
            Log.Debug("ReadFile - файл не существует.");
            return default(T);
        }
    }
}
