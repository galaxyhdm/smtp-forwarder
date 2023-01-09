using System.Reflection;
using Flurl;
using Flurl.Http;
using SmtpForwarder.ForwardingApi;

namespace SmtpForwarder.TelegramForwarder;

[Forwarding("telegram_forwarder")]
public class TelegramForwarder : IForwarder
{
    public string Name => (GetType().GetCustomAttribute(typeof(ForwardingAttribute)) as ForwardingAttribute)!.Name;

    private string _chatId;
    
    public Task InitializeAsync(dynamic forwarderConfig)
    {
        _chatId = forwarderConfig.ChatId;
        return Task.CompletedTask;
    }

    public Task ForwardMessage()
    {
        return Task.FromResult("");
    }

}