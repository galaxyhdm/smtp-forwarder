using System.Diagnostics.CodeAnalysis;
using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Events.ForwardingAddressEvents;
using SmtpForwarder.Application.Events.PermissionEvents;
using SmtpForwarder.Application.Extensions;
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

    public InternalForwardingHandler(IMediator mediator)
    {
        _mediator = mediator;
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
        PrintNotInDatabase(localParts, forwardingAddresses);

        var allowedAddresses = await CheckPermission(cancellationToken, forwardingAddresses, mailBox);
        PrintMissingPermission(forwardingAddresses, allowedAddresses);

        //Todo: Make env:
        const string mainFolder = "files";
        var folder = Path.Combine(mainFolder, requestId.ToString());

        foreach (var messageAttachment in message.Attachments.OfType<MimePart>())
        {
            var id = messageAttachment.ContentId.EscapePath();
            var filePath = Path.Combine(folder, $"{id}");
            Log.Info(File.Exists(filePath));
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

    private void PrintNotInDatabase(IEnumerable<string> all, IEnumerable<ForwardingAddress> found)
    {
        foreach (var address in all.Except(found.Select(address => address.LocalAddressPart)))
            Log.Trace("Ignoring internal mail address {}, db-entry not found!",
                address);
    }

    private void PrintMissingPermission(IEnumerable<ForwardingAddress> all, IEnumerable<ForwardingAddress> allowed)
    {
        foreach (var address in all.Except(allowed))
            Log.Trace("Ignoring internal mail address {} ({}), because of missing permissions!",
                address.LocalAddressPart, address.Id);
    }
}