using System.Runtime.ConstrainedExecution;
using System.Text.Json;

namespace Keki {
    internal class Settings {
        public required String Cake { get; set; }
        private static JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public static Settings GetDefault() {
            return new Settings {
                Cake = "keki"
            };
        }

        public static Settings Load(string path) {
            Settings? result;
            if (!File.Exists(path)) {
                result = GetDefault();
                File.WriteAllText(path, JsonSerializer.Serialize(result, jsonOptions));
                return result;
            }
            result = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path));
            if (result is not Settings) {
                return GetDefault();
            }
            return result;
        }

        public void Save(string path) {
            File.WriteAllText(path, JsonSerializer.Serialize(this, jsonOptions));
        }
    }
}
