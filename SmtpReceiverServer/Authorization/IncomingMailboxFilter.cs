using MediatR;
using NLog;
using SmtpForwarder.Application;
using SmtpForwarder.Domain;
using SmtpForwarder.SmtpReceiverServer.Extensions;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpForwarder.SmtpReceiverServer.Authorization;

internal class IncomingMailboxFilter : IMailboxFilter
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public IncomingMailboxFilter(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size,
        CancellationToken cancellationToken)
    {
        if (from == Mailbox.Empty)
            return Task.FromResult(MailboxFilterResult.NoPermanently);

        if (!context.Properties.TryGetValue(Constants.InternalMailBoxKey, out MailBox? mailBox))
            return Task.FromResult(MailboxFilterResult.NoPermanently);

        if (mailBox == null)
            return Task.FromResult(MailboxFilterResult.NoPermanently);

        if (mailBox.LocalAddressPart.Equals(from.User, StringComparison.OrdinalIgnoreCase)
            && from.Host.Equals("test.lab"))
            return Task.FromResult(MailboxFilterResult.Yes);

        Log.Debug(
            $"Incoming mail-address does not match db-entry, abort! ({from.User}@{from.Host}" +
            $" | {mailBox.LocalAddressPart} = {mailBox.MailAddress})");
        return Task.FromResult(MailboxFilterResult.NoPermanently);
    }

    public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(MailboxFilterResult.Yes);
    }
}