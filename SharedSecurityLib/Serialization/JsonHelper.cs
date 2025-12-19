using System.Text.Json;

namespace SharedSecurityLib.Serialization
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options =
            new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        // Herhangi bir nesneyi JSON string'e çevirir
        // Serializes any object to JSON string
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        // JSON string'den nesne oluşturur
        // Deserializes JSON string into an object
        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}
