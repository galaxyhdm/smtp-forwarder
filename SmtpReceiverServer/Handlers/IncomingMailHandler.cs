using System.Buffers;
using MediatR;
using NLog;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpReceiverServer.Handlers;

internal class IncomingMailHandler : IMessageStore
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public IncomingMailHandler(IMediator mediator) {
        _mediator = mediator;
    }
    
    public Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}