using MediatR;
using Microsoft.Extensions.Options;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Filter;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Jobs;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;
using SmtpForwarder.Domain.Settings;

namespace SmtpForwarder.Application.Services;

internal class IncomingMessageService : IIncomingMessageService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly string _internalDomainPart;
    private readonly bool _allowSmtpForward;
    private readonly List<string> _allowedForwardAddresses;
    
    private readonly IMailBoxRepository _mailBoxRepository;
    private readonly IForwardingAddressRepository _forwardingAddressRepository;
    private readonly IForwardingService _forwardingService;
    private readonly IMediator _mediator;

    public IncomingMessageService(IMailBoxRepository mailBoxRepository,
        IForwardingAddressRepository forwardingAddressRepository, IMediator mediator,
        IForwardingService forwardingService,
        IOptions<Settings> settingOptions)
    {
        _mailBoxRepository = mailBoxRepository;
        _forwardingAddressRepository = forwardingAddressRepository;
        _mediator = mediator;
        _forwardingService = forwardingService;
        
        _internalDomainPart = settingOptions.Value.InternalDomain;
        _allowSmtpForward = settingOptions.Value.AllowSmtpForward;
        _allowedForwardAddresses = settingOptions.Value.AllowedForwardDomains;
    }

    public async Task<IncomingMessageResponse> HandleIncomingMessage(MailBox mailBox, MimeMessage message,
        CancellationToken token)
    {
        var mailboxAddresses = message.GetRecipients(true).ToHashSet();

        var recipients = RecipientFilter.SortRecipients(
            mailboxAddresses,
            _internalDomainPart,
            _allowSmtpForward,
            _allowedForwardAddresses); //SortRecipients(message, mailBox);

        if (recipients.Count == 0)
        {
            Log.Debug("No recipients found!");
            ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Warn, "handling", "no-valid-1",
                "No recipients found", end: true);
            return IncomingMessageResponse.NoValidRecipientsGiven;
        }

        var countAccepted = recipients.ExceptBy(new[] {MailAddressType.Blocked}, pair => pair.Key).Count();

        PrintBlocked(recipients, message.MessageId);

        if (countAccepted == 0)
        {
            Log.Debug("No recipients found!");
            ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Warn, "handling", "no-valid-2",
                "No recipients found", end: true);
            return IncomingMessageResponse.NoValidRecipientsGiven;
        }

        _forwardingService.EnqueueForwardingRequest(new ForwardingRequest(mailBox, message, recipients));

        return IncomingMessageResponse.Ok;
    }

    private void PrintBlocked(Dictionary<MailAddressType, List<MailboxAddress>> dictionary, string messageId)
    {
        if (!dictionary.TryGetValue(MailAddressType.Blocked, out var blockedAddresses)) return;
        foreach (var address in blockedAddresses)
        {
            Log.Warn("Ignoring mail address ({}), because smtp-forward is not allowed for domain: {}",
                address.Address,
                address.Domain);

            ProcessTraceBucket.Get.LogTrace(messageId, TraceLevel.Warn, "handling", "block-address",
                $"Ignoring mail address ({address.Address}), because smtp-forward is not allowed for domain: {address.Domain}");
        }
    }

}