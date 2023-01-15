using System.Net;
using SmtpServer;

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
    
    public static string GetIpString(this ISessionContext sessionContext)
    {
        if (!sessionContext.Properties.TryGetValue("EndpointListener:RemoteEndPoint", out var value) || value is null)
            return "unknown";
        return value is not IPEndPoint endPoint ? "unknown" : endPoint.ToString();
    }
    
    public static IPEndPoint GetIp(this ISessionContext sessionContext)
    {
        if (!sessionContext.Properties.TryGetValue("EndpointListener:RemoteEndPoint", out var value) || value is null)
            throw new NullReferenceException("IPEndPoint not found!");
        return value as IPEndPoint ?? throw new NullReferenceException("IPEndPoint not found!");
    }
    
}