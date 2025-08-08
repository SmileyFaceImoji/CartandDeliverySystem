using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }

    public static void SetDouble(this ISession session, string key, double value)
    {
        session.SetString(key, value.ToString());
    }

    public static double GetDouble(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? 0.0 : double.Parse(value);
    }

    public static void SetBoolean(this ISession session, string key, bool value)
    {
        session.SetString(key, value.ToString());
    }

    public static bool GetBoolean(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? false : bool.Parse(value);
    }

    public static void SetDateTime(this ISession session, string key, DateTime value)
    {
        session.SetString(key, value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    public static DateTime GetDateTime(this ISession session, string key)
    {
        var value = session.GetString(key);
        return string.IsNullOrEmpty(value) ? DateTime.MinValue : DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss", null);
    }
}
