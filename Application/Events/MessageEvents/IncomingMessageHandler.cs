using MediatR;
using MimeKit;

namespace SmtpForwarder.Application.Events.MessageEvents;

public record IncomingMessageRequest(MimeMessage Message) : IRequest<IncomingMessageResponse>;

public class IncomingMessageHandler : IRequestHandler<IncomingMessageRequest, IncomingMessageResponse>
{

    public async Task<IncomingMessageResponse> Handle(IncomingMessageRequest request, CancellationToken cancellationToken)
    {
        return IncomingMessageResponse.Ok;
    }
}

public enum IncomingMessageResponse
{
    Ok,
    MailboxNameNotAllowed,
    MailboxUnavailable,
    NoValidRecipientsGiven,
    SizeLimitExceeded,
    Error,
}