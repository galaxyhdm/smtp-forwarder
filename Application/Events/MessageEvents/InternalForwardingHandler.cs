using System.Diagnostics.CodeAnalysis;
using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Events.ForwardingAddressEvents;
using SmtpForwarder.Application.Events.PermissionEvents;
using SmtpForwarder.Application.Extensions;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.MessageEvents;

public record InternalForwardingRequest
    (MailBox MailBox, MimeMessage Message, List<MailboxAddress> Addresses, Guid RequestId) : IRequest<bool>;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public class InternalForwardingHandler : IRequestHandler<InternalForwardingRequest, bool>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static string InternalDomainPart = "test.lab";

    private readonly IMediator _mediator;
    private readonly IForwardingController _forwardingController;

    public InternalForwardingHandler(IMediator mediator, IForwardingController forwardingController)
    {
        _mediator = mediator;
        _forwardingController = forwardingController;
    }

    public async Task<bool> Handle(InternalForwardingRequest request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var mailBox, out var message, out var addresses, out var requestId);

        if (addresses.Count == 0) return false;

        var localParts = addresses
            .Where(address => address.Domain.Equals(InternalDomainPart))
            .Select(address => address.LocalPart).ToList();

        // Get corresponding database entries 
        var forwardingAddresses = await _mediator.Send(new GetForwardingAddressByList(localParts), cancellationToken);
        PrintNotInDatabase(localParts, forwardingAddresses, message.MessageId);

        var allowedAddresses = await CheckPermission(cancellationToken, forwardingAddresses, mailBox);
        PrintMissingPermission(forwardingAddresses, allowedAddresses, message.MessageId);

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

        foreach (var forwardingAddress in allowedAddresses)
        {
            if (!forwardingAddress.ForwardTargetId.HasValue)
            {
                Log.Warn(
                    "Could not forward message ({} | {}) with forwarding address ({} | {}), because no forwarding-target is assigned!",
                    requestId, message.MessageId, forwardingAddress.LocalAddressPart, forwardingAddress.Id);

                ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Warn, "forwarding", "try-forward-1",
                    $"Could not forward message with forwarding address ({forwardingAddress.LocalAddressPart}), because no forwarding-target is assigned!");
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

        //todo: handle forward
        return true;
    }

    private async Task<List<ForwardingAddress>> CheckPermission(CancellationToken cancellationToken,
        IEnumerable<ForwardingAddress> forwardingAddresses, MailBox mailBox)
    {
        var allowedAddresses = new List<ForwardingAddress>();
        foreach (var forwardingAddress in forwardingAddresses)
        {
            var result = await _mediator.Send(
                new ForwardingAddressPermissionCheck(mailBox.Owner, forwardingAddress),
                cancellationToken);
            if (!result) continue;
            allowedAddresses.Add(forwardingAddress);
        }

        return allowedAddresses;
    }

    private void PrintNotInDatabase(IEnumerable<string> all, IEnumerable<ForwardingAddress> found,
        string messageMessageId)
    {
        foreach (var address in all.Except(found.Select(address => address.LocalAddressPart)))
        {
            Log.Trace("Ignoring internal mail address {}, db-entry not found!",
                address);

            ProcessTraceBucket.Get.LogTrace(messageMessageId, TraceLevel.Warn, "forwarding", "not-found-1",
                $"Could not found internal mail address: {address}");
        }
    }

    private void PrintMissingPermission(IEnumerable<ForwardingAddress> all, IEnumerable<ForwardingAddress> allowed,
        string messageMessageId)
    {
        foreach (var address in all.Except(allowed))
        {
            Log.Trace("Ignoring internal mail address {} ({}), because of missing permissions!",
                address.LocalAddressPart, address.Id);

            ProcessTraceBucket.Get.LogTrace(messageMessageId, TraceLevel.Warn, "forwarding", "not-found-2",
                $"Could not found internal mail address: {address.LocalAddressPart}");
        }
    }
}