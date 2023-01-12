using MediatR;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Events.MessageEvents;
using SmtpForwarder.Application.Extensions;
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
        Log.Info("Starting email forwarding for mail: {} ({})", _message.MessageId, RequestId);
        var startTime = DateTime.UtcNow;
        
        Log.Trace("Extracting attachments");
        await ExtractAttachments();
        
        var internalAddresses = GetAddresses(MailAddressType.Internal);
        var externalAddresses = GetAddresses(MailAddressType.ForwardExternal);

        await _mediator.Send(new InternalForwardingRequest(_mailBox, _message, internalAddresses, RequestId));

        var stopTime = DateTime.UtcNow;
        Log.Info("Finished email forwarding for mail: {} ({}) in {}ms", _message.MessageId, RequestId, (stopTime-startTime).TotalMilliseconds);

        await CleanUp();
        DisposeMimeMessage(message: _message);
    }

    private List<MailboxAddress> GetAddresses(MailAddressType addressType)
    {
        return _recipients.TryGetValue(addressType, out var addressesTemp)
            ? addressesTemp
            : new List<MailboxAddress>();
    }

    internal void SetMediator(IMediator mediator) =>
        _mediator = mediator;

    private async Task ExtractAttachments()
    {
        //Todo: Make env:
        const string mainFolder = "files";
        var folder = Path.Combine(mainFolder, RequestId.ToString());

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        
        foreach (var part in _message.Attachments.OfType<MimePart>())
        {
            if(!part.IsAttachment) continue;
            var id = part.ContentId.EscapePath();
            var filePath = Path.Combine(folder, $"{id}.temp");

            // Write raw MimePart data
            await part.WriteToAsync(filePath, false);
            
            // Write real file
            await using var downloadFile = File.Create(Path.Combine(folder, $"{id}"));
            await part.Content.DecodeToAsync(downloadFile);
        }
    }

    private async Task CleanUp()
    {
        //Todo: Make env:
        const string mainFolder = "files";
        var folder = Path.Combine(mainFolder, RequestId.ToString());
     
        if(!Directory.Exists(folder)) return;
        Directory.Delete(folder, true);
    }
    
    static void DisposeMimeMessage(MimeMessage message)
    {
        foreach (var bodyPart in message.BodyParts)
        {
            if (bodyPart is MessagePart rfc822)
            {
                DisposeMimeMessage(rfc822.Message);
            }
            else
            {
                var part = (MimePart) bodyPart;
                part.Content.Stream.Dispose();
            }
        }

        message.Dispose();
        //GC.Collect();
    }
}