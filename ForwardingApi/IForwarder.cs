using MimeKit;

namespace SmtpForwarder.ForwardingApi;

public interface IForwarder
{
    Task InitializeAsync(string forwarderConfig);

    Task ForwardMessage(MimeMessage message, List<string> attachmentIds, Guid requestId);
    
    string Name { get; }
}