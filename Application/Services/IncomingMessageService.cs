using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Extensions;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Jobs;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Services;

internal class IncomingMessageService : IIncomingMessageService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    //todo get from env
    private static string InternalDomainPart = "test.lab";
    private static bool AllowSmtpForward = true;
    private static List<string> AllowedForwardAddresses = new() {"hallo.lab"};

    private readonly IMailBoxRepository _mailBoxRepository;
    private readonly IForwardingAddressRepository _forwardingAddressRepository;
    private readonly IForwardingService _forwardingService;
    private readonly IMediator _mediator;

    public IncomingMessageService(IMailBoxRepository mailBoxRepository,
        IForwardingAddressRepository forwardingAddressRepository, IMediator mediator,
        IForwardingService forwardingService)
    {
        _mailBoxRepository = mailBoxRepository;
        _forwardingAddressRepository = forwardingAddressRepository;
        _mediator = mediator;
        _forwardingService = forwardingService;
    }

    public async Task<IncomingMessageResponse> HandleIncomingMessage(MailBox mailBox, MimeMessage message,
        CancellationToken token)
    {
        var recipients = SortRecipients(message, mailBox);

        if (recipients.Count == 0)
        {
            Log.Debug("No recipients found!");
            return IncomingMessageResponse.NoValidRecipientsGiven;
        }
        
        var countAccepted = recipients.ExceptBy(new[] {MailAddressType.Blocked}, pair => pair.Key).Count();
        
        PrintBlocked(recipients);
        
        if (countAccepted == 0)
        {
            Log.Debug("No recipients found!");
            return IncomingMessageResponse.NoValidRecipientsGiven;
        }

        _forwardingService.EnqueueForwardingRequest(new ForwardingRequest(mailBox, message, recipients));
        
        return IncomingMessageResponse.Ok;
    }

    private Dictionary<MailAddressType, List<MailboxAddress>> SortRecipients(MimeMessage message, MailBox mailBox)
    {
        var dictionary = new Dictionary<MailAddressType, List<MailboxAddress>>();
        var recipients = message.GetRecipients(true);
        if (recipients is null || recipients.Count == 0)
            return dictionary;

        foreach (var address in recipients)
        {
            if (address.Domain.Equals(InternalDomainPart))
            {
                dictionary.AddToDictionary(MailAddressType.Internal, address);
                continue;
            }

            if (AllowSmtpForward && (AllowedForwardAddresses.Contains("*") ||
                                     AllowedForwardAddresses.Contains(address.Domain)))
            {
                dictionary.AddToDictionary(MailAddressType.ForwardExternal, address);
                continue;
            }
            
            dictionary.AddToDictionary(MailAddressType.Blocked, address);
        }

        return dictionary;
    }

    private void PrintBlocked(Dictionary<MailAddressType, List<MailboxAddress>> dictionary)
    {
        if(!dictionary.TryGetValue(MailAddressType.Blocked, out var blockedAddresses)) return;
        foreach (var address in blockedAddresses)
        {
            Log.Warn("Ignoring mail address ({}), because smtp-forward is not allowed for domain: {}",
                address.Address,
                address.Domain);
        }
    }

}