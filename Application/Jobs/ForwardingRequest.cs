using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Jobs;

public class ForwardingRequest
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public readonly Guid RequestId = Guid.NewGuid();

    private IMediator _mediator;
    private readonly MailBox _mailBox;
    private readonly MimeMessage _message;
    private readonly Dictionary<MailAddressType, List<MailboxAddress>> _recipients;

    public ForwardingRequest(MailBox mailBox, MimeMessage message,
        Dictionary<MailAddressType, List<MailboxAddress>> recipients)
    {
        _mailBox = mailBox;
        _message = message;
        _recipients = recipients;
    }

    public async Task Run()
    {
        Log.Trace("Running forwarding request: {}", RequestId);
        var internalAddresses = _recipients.TryGetValue(MailAddressType.Internal, out var internalAddressesTemp)
            ? internalAddressesTemp
            : new List<MailboxAddress>();

        
    }

    internal void SetMediator(IMediator mediator) =>
        _mediator = mediator;
}