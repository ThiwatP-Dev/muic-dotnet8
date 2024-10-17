public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        var result = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        var result = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
        return value == null ? default : JsonConvert.DeserializeObject<T>(value, result);
    }
}