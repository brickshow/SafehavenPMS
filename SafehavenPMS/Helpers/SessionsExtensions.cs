using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SafehavenPMS.Helpers
{
    public static class SessionsExtensions
    {
        // Generic method to store objects in session after serializing to JSON
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            // Convert object to JSON string and store in session
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Generic method to retrieve and deserialize objects from session
        public static T GetObject<T>(this ISession session, string key)
        {
            // Get the JSON string from session
            var value = session.GetString(key);
            // If value exists, deserialize it; otherwise return default value of T
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
