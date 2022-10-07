using MimeKit;
using SmtpForwarder.Application.Enums;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Interfaces.Services;

public interface IIncomingMessageService
{
    Task<IncomingMessageResponse> HandleIncomingMessage(MailBox mailBox, MimeMessage message, CancellationToken token);
}