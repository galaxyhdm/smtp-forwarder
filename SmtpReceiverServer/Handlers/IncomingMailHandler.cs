using System.Buffers;
using MediatR;
using NLog;
using SmtpForwarder.Application;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Events.MessageEvents;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;
using SmtpForwarder.Domain.Enums;
using SmtpForwarder.SmtpReceiverServer.Extensions;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpForwarder.SmtpReceiverServer.Handlers;

internal class IncomingMailHandler : IMessageStore
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public IncomingMailHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        if (!context.Authentication.IsAuthenticated)
            return SmtpResponse.AuthenticationRequired;

        if (!context.Properties.TryGetValue(Constants.InternalMailBoxKey, out MailBox? mailBox))
            return SmtpResponse.AuthenticationFailed;

        if (mailBox is null)
            return SmtpResponse.AuthenticationFailed;

        var message = await buffer.TryToMimeMessageAsync(cancellationToken);
        if (message is null)
            return SmtpResponse.SyntaxError;

        var messageId = message.MessageId;

        if (context.Properties.TryGetValue(Constants.SessionStartKey, out DateTime? startTime) && startTime.HasValue)
        {
            var handlingTime = (DateTime.UtcNow - startTime.Value).TotalMilliseconds;
            Log.Info("Finished smtp request from {} with message {} in {}ms",
                context.GetIpString(),
                messageId,
                handlingTime);
            ProcessTraceBucket.Get.LogTrace(messageId, TraceLevel.Info, "smtp", "finished-smtp",
                $"Got smtp request in {handlingTime}ms");
        }

        Log.Debug($"Handling incoming message ({messageId}) from {transaction.From.AsAddress()}");

        
        //Log.Trace($"Subject={message.Subject}");
        //Log.Trace($"Body={message.TextBody}");

        var response = await _mediator.Send(new IncomingMessageRequest(mailBox, message), cancellationToken);

        return response switch
        {
            IncomingMessageResponse.Ok => SmtpResponse.Ok,
            IncomingMessageResponse.MailboxNameNotAllowed => SmtpResponse.MailboxNameNotAllowed,
            IncomingMessageResponse.MailboxUnavailable => SmtpResponse.MailboxUnavailable,
            IncomingMessageResponse.NoValidRecipientsGiven => SmtpResponse.NoValidRecipientsGiven,
            IncomingMessageResponse.SizeLimitExceeded => SmtpResponse.SizeLimitExceeded,
            _ => SmtpResponse.TransactionFailed
        };
    }
}