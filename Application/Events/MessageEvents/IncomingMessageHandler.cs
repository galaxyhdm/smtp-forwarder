using MediatR;
using MimeKit;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.MessageEvents;

public record IncomingMessageRequest(MailBox MailBox, MimeMessage Message) : IRequest<IncomingMessageResponse>;

public class IncomingMessageHandler : IRequestHandler<IncomingMessageRequest, IncomingMessageResponse>
{

    private readonly IIncomingMessageService _messageService;

    public IncomingMessageHandler(IIncomingMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task<IncomingMessageResponse> Handle(IncomingMessageRequest request,
        CancellationToken cancellationToken)
    {
        return await _messageService.HandleIncomingMessage(request.MailBox, request.Message, cancellationToken);
    }
}