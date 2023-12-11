
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoomRental.Infrastructure
{
    public static class SessionExtensions
    {
        //Запись объекта типа Dictionary<string, string> в сессию
        public static void Set(this ISession session, string key, Dictionary<string, string> dictionary)
        {
            session.SetString(key, JsonSerializer.Serialize(dictionary));
        }
        //Считывание объекта типа Dictionary<string, string> из сессии
        public static Dictionary<string, string> Get(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(Dictionary<string, string>) : JsonSerializer.Deserialize<Dictionary<string, string>>(value);
        }
    }
}
