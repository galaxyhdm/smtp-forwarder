namespace Application.Utils;

public class Env
{
    public static string GetStringRequired(string key, string defaultValue = "")
    {
        return GetStringDefault(key, defaultValue) ?? string.Empty;
    }
    
    public static string? GetStringDefault(string key, string? defaultValue = "") =>
        Environment.GetEnvironmentVariable(key) ?? defaultValue;

    public static int GetIntDefault(string key, int defaultValue = 0, Func<int, bool>? valid = null)
    {
        var value = GetStringDefault(key, null);
        if (value == null) return defaultValue;
        var result = int.TryParse(value, out var integer) ? integer : defaultValue;
            
        return valid != null && !valid(result) ? defaultValue : result;
    }

    public static bool GetBoolDefault(string key, bool defaultValue = false) {
        var value = GetStringDefault(key, null);
        if (value == null) return defaultValue;

        if (bool.TryParse(value, out var boolResult)) return boolResult;
        return value.Equals("1") || value.Equals("T", StringComparison.CurrentCultureIgnoreCase);
    }
}