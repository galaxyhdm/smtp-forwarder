using MimeKit;
using NLog;

namespace SmtpForwarder.Application.Jobs;

public class ForwardingRequest
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public readonly Guid RequestId = Guid.NewGuid();

    private readonly MimeMessage _message;
    private readonly List<MailboxAddress> _recipients;

    public ForwardingRequest(MimeMessage message, List<MailboxAddress> recipients)
    {
        _message = message;
        _recipients = recipients;
    }

    public async Task Run()
    {
        
    }
}