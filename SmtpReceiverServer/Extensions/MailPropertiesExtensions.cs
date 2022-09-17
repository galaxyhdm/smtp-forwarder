namespace SmtpForwarder.SmtpReceiverServer.Extensions ;

internal static class MailPropertiesExtensions {

    public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T? tValue) {
        if (!dictionary.TryGetValue(key, out var value)) {
            tValue = default;
            return false;
        }

        try {
            tValue = (T) value;
            return true;
        }
        catch (Exception) {
            tValue = default;
            return false;
        }
    }
        
}