using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Jobs;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Services;

internal class IncomingMessageService : IIncomingMessageService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    //todo get from env
    private static string DomainPart = "test.lab";
    private static bool AllowSmtpForward = true;
    private static List<string> AllowedForwardAddresses = new() {"hallo.lab"};

    private readonly IMailBoxRepository _mailBoxRepository;
    private readonly IForwardingAddressRepository _forwardingAddressRepository;
    private readonly IForwardingService _forwardingService;
    private readonly IMediator _mediator;

    public IncomingMessageService(IMailBoxRepository mailBoxRepository,
        IForwardingAddressRepository forwardingAddressRepository, IMediator mediator, IForwardingService forwardingService)
    {
        _mailBoxRepository = mailBoxRepository;
        _forwardingAddressRepository = forwardingAddressRepository;
        _mediator = mediator;
        _forwardingService = forwardingService;
    }

    public async Task<IncomingMessageResponse> HandleIncomingMessage(MailBox mailBox, MimeMessage message,
        CancellationToken token)
    {
        var recipients = CheckAndRemoveRecipients(message, mailBox);

        if (recipients.Count == 0)
        {
            Log.Debug("No recipients found!");
            return IncomingMessageResponse.NoValidRecipientsGiven;
        }

        _forwardingService.EnqueueForwardingRequest(new ForwardingRequest(message, recipients));
        
        return IncomingMessageResponse.Ok;
    }

    private List<MailboxAddress> CheckAndRemoveRecipients(MimeMessage message, MailBox mailBox)
    {
        var recipients = message.GetRecipients(true);
        if (recipients is null || recipients.Count == 0)
            return new List<MailboxAddress>();

        List<MailboxAddress> sortedRecipients =
            AllowSmtpForward
                ? recipients.Where(address =>
                    AllowedForwardAddresses.Contains("*")
                    || AllowedForwardAddresses.Contains(address.Domain)
                    || address.Domain.Equals(DomainPart)).ToList()
                : recipients.Where(address => address.Domain.Equals(DomainPart)).ToList();

        foreach (var mailboxAddress in recipients.Except(sortedRecipients))
        {
            Log.Debug("Ignoring mail address ({}), because smtp-forward is not allowed for domain: {}",
                mailboxAddress.Address,
                mailboxAddress.Domain);
        }

        return sortedRecipients;
    }
    
}