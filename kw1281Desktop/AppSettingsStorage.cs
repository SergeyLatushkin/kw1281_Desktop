using System.Text.Json;

namespace kw1281Desktop
{

    public static class AppSettingsStorage
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "kw1281",
            "settings.json"
        );

        public static void Save<T>(string key, T value)
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            var data = Load() ?? new ();

            data[key] = value!;

            File.WriteAllText(FilePath, JsonSerializer.Serialize(new SerializableSettings { Settings = data }));
        }

        public static Dictionary<string, object>? Load()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            var json = File.ReadAllText(FilePath);
            var data = JsonSerializer.Deserialize<SerializableSettings>(json);
            if (data is null)
            {
                return null;
            }

            return data.Settings;
        }

        private class SerializableSettings
        {
            public Dictionary<string, object> Settings { get; set; } = new ();
        }
    }
}
