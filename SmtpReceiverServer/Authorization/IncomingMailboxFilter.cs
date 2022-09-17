using MediatR;
using NLog;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace SmtpForwarder.SmtpReceiverServer.Authorization;

internal class IncomingMailboxFilter : IMailboxFilter
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public IncomingMailboxFilter(IMediator mediator) {
        _mediator = mediator;
    }

    public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}