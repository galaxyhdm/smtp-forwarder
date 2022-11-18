namespace SmtpForwarder.Application.Extensions;

public static class DictionaryExtension
{

    public static void AddToDictionary<TK, TV>(this Dictionary<TK, List<TV>> dictionary, TK key, TV value)
        where TK : notnull
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, new List<TV> {value});
            return;
        }

        var list = dictionary[key];
        list.Add(value);
    }

}