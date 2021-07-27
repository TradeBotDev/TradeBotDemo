using System.IO;
using System.Text.Json;
using Serilog;

namespace AccountGRPC
{
    public static class FileManagement
    {
        // Метод записи объекта любого типа в файл.
        public static bool WriteFile<T>(string filename, T state)
        {
            // В случае, если объект, который требуется записать, является пустым, отправляется
            // false, что означает, что запись не была выполнена.
            if (state == null)
            {
                Log.Information($"Данные не записаны в файл {filename}, так как объект является пустым.");
                return false;
            }
            
            // Иначе данные объекта записываются в файл.
            string serialized = JsonSerializer.Serialize(state);
            File.WriteAllText(filename, serialized);
            Log.Information($"Данные записаны в файл {filename}.");
            return true;
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
