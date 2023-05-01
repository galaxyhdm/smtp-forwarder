using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Options;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Events.ForwardingAddressEvents;
using SmtpForwarder.Application.Events.PermissionEvents;
using SmtpForwarder.Application.Extensions;
using SmtpForwarder.Application.Filter;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;
using SmtpForwarder.Domain.Settings;
using TraceLevel = SmtpForwarder.Application.Utils.TraceLevel;

namespace SmtpForwarder.Application.Events.MessageEvents;

public record InternalForwardingRequest
    (MailBox MailBox, MimeMessage Message, List<MailboxAddress> Addresses, Guid RequestId) : IRequest<bool>;

public class InternalForwardingHandler : IRequestHandler<InternalForwardingRequest, bool>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly string _internalDomainPart;

    private readonly IMediator _mediator;
    private readonly IForwardingController _forwardingController;

    public InternalForwardingHandler(IMediator mediator, IForwardingController forwardingController,
        IOptions<Settings> settingOptions)
    {
        _mediator = mediator;
        _forwardingController = forwardingController;
        _internalDomainPart = settingOptions.Value.InternalDomain;

        Log.Info("Using {} as internal domain.", _internalDomainPart);
    }

    public async Task<bool> Handle(InternalForwardingRequest request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var mailBox, out var message, out var addresses, out var requestId);

        if (addresses.Count == 0) return false;

        // Map to filter objects
        var filterAddresses = addresses.Select(address => new RecipientFilterAddress(address)).ToList();
        var filterLocalParts = filterAddresses.Select(address => address.LocalPart).ToList();

        // Get corresponding database entries 
        var forwardingAddresses =
            await _mediator.Send(new GetForwardingAddressByList(filterLocalParts), cancellationToken);


        async Task<RecipientFilterAddress> SetPermission(RecipientFilterAddress address)
        {
            address.HasPermission = await CheckPermission(address.ForwardingAddress, mailBox, cancellationToken);
            return address;
        }

        var allowedAddresses = filterAddresses.Select(address => (
                Address: address,
                ForwardingAddress: forwardingAddresses.FirstOrDefault(forwardingAddress =>
                    forwardingAddress.LocalAddressPart == address.LocalPart)
            ))
            .Select(tuple => tuple.Address.SetForwardingAddress(tuple.ForwardingAddress))
            .Where(address => address.InDatabase)
            .Select(async t => await SetPermission(t))
            .Select(task => task.Result)
            .Where(address => address.HasPermission)
            .ToList();

        PrintMessages(filterAddresses, message);

        var attachmentIds = GetAttachmentIds(requestId, message);

        foreach (var recipientFilterAddress in allowedAddresses.Where(address => address.ForwardingAddress is not null))
        {
            var forwardingAddress = recipientFilterAddress.ForwardingAddress!;

            if (!forwardingAddress.ForwardTargetId.HasValue)
            {
                PrintTargetNotAssigned(requestId, message, forwardingAddress);
                continue;
            }

            Log.Debug("Forwarding message ({}) with forwarding address ({} | {}) to target: {}",
                message.MessageId,
                forwardingAddress.LocalAddressPart, forwardingAddress.Id,
                forwardingAddress.ForwardTargetId.Value);

            ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Info, "forwarding", "try-forward-2",
                $"Forwarding message with forwarding address ({forwardingAddress.LocalAddressPart})");

            await _forwardingController.GetForwarder(forwardingAddress.ForwardTargetId.Value)
                .ForwardMessage(message, attachmentIds, requestId);
        }

        return true;
    }

    private static List<string> GetAttachmentIds(Guid requestId, MimeMessage message)
    {
        //Todo: Make env:
        const string mainFolder = "files";
        var folder = Path.Combine(mainFolder, requestId.ToString());

        var attachmentIds = new List<string>();
        foreach (var messageAttachment in message.Attachments.OfType<MimePart>())
        {
            var id = messageAttachment.ContentId.EscapePath();
            var filePath = Path.Combine(folder, $"{id}");
            if (File.Exists(filePath))
                attachmentIds.Add(id);
        }

        return attachmentIds;
    }

    private async Task<bool> CheckPermission(ForwardingAddress? forwardingAddress,
        MailBox mailBox, CancellationToken cancellationToken)
    {
        if (forwardingAddress is null) return false;
        return await _mediator.Send(
            new ForwardingAddressPermissionCheck(mailBox.Owner, forwardingAddress),
            cancellationToken);
    }

    private static void PrintMessages(List<RecipientFilterAddress> filterAddresses, MimeMessage message)
    {
        foreach (var recipientFilterAddress in filterAddresses)
        {
            if (!recipientFilterAddress.InDatabase)
                PrintNotInDatabase(recipientFilterAddress.LocalPart, message.MessageId);
            else if (!recipientFilterAddress.HasPermission)
                PrintMissingPermission(recipientFilterAddress.ForwardingAddress, message.MessageId);
        }
    }

    private static void PrintNotInDatabase(string address, string messageMessageId)
    {
        Log.Trace("Ignoring internal mail address {}, db-entry not found!",
            address);

        ProcessTraceBucket.Get.LogTrace(messageMessageId, TraceLevel.Warn, "forwarding", "not-found-1",
            $"Could not found internal mail address: {address}");
    }

    private static void PrintMissingPermission(ForwardingAddress? address, string messageMessageId)
    {
        Debug.Assert(address != null, nameof(address) + " != null");
        Log.Trace("Ignoring internal mail address {} ({}), because of missing permissions!",
            address.LocalAddressPart, address.Id);

        ProcessTraceBucket.Get.LogTrace(messageMessageId, TraceLevel.Warn, "forwarding", "not-found-2",
            $"Could not found internal mail address: {address.LocalAddressPart}");
    }

    private static void PrintTargetNotAssigned(Guid requestId, MimeMessage message, ForwardingAddress forwardingAddress)
    {
        Log.Warn(
            "Could not forward message ({} | {}) with forwarding address ({} | {}), because no forwarding-target is assigned!",
            requestId, message.MessageId, forwardingAddress.LocalAddressPart, forwardingAddress.Id);

        ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Warn, "forwarding", "try-forward-1",
            $"Could not forward message with forwarding address ({forwardingAddress.LocalAddressPart}), because no forwarding-target is assigned!");
    }

}