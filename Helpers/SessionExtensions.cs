using System.Text.Json;

namespace JoyeriaMorgan.Helpers;

public static class SessionExtensions
{
    public static void SetObjeto<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObjeto<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}