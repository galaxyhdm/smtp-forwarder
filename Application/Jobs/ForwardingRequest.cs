using MediatR;
using MimeKit;
using MimeKit.Utils;
using NLog;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Events.MessageEvents;
using SmtpForwarder.Application.Extensions;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Jobs;

public class ForwardingRequest
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public readonly Guid RequestId = Guid.NewGuid();
    public MimeMessage Message => _message;

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
        ProcessTraceBucket.Get.LogTrace(_message.MessageId, TraceLevel.Info, "forwarding", "start-forwarding",
            "Starting email forwarding");

        var startTime = DateTime.UtcNow;

        Log.Trace("Extracting attachments");
        await ExtractAttachments();

        var internalAddresses = GetAddresses(MailAddressType.Internal);
        var externalAddresses = GetAddresses(MailAddressType.ForwardExternal);

        var internalTask =
            _mediator.Send(new InternalForwardingRequest(_mailBox, _message, internalAddresses, RequestId));

        var externalTask = _mediator.Send(new ExternalForwardingRequest(RequestId, _message, externalAddresses));
        
        await Task.WhenAll(
            internalTask,
            externalTask);

        var stopTime = DateTime.UtcNow;
        var forwardingTime = (stopTime - startTime).TotalMilliseconds;
        Log.Info("Finished email forwarding for mail: {} ({}) in {}ms", _message.MessageId, RequestId,
            forwardingTime);
        ProcessTraceBucket.Get.EndTraceLog(_message.MessageId, TraceLevel.Info, "forwarding", "end-forwarding",
            $"Finished email forwarding in {forwardingTime}ms", _mailBox);

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
            if (!part.IsAttachment) continue;
            if (string.IsNullOrWhiteSpace(part.ContentId))
                // Generate a custom contentId every time, to prevent injections and bad chars
                part.ContentId = MimeUtils.GenerateMessageId();
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

        if (!Directory.Exists(folder)) return;
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
                part.Content.Dispose();
            }
        }

        message.Dispose();
        //GC.Collect();
    }
}