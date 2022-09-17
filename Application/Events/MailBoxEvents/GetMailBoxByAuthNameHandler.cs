using MediatR;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.MailBoxEvents;

public record GetMailBoxByAuthName(string AuthName) : IRequest<MailBox?>;

public class GetMailBoxByAuthNameHandler : IRequestHandler<GetMailBoxByAuthName, MailBox?>
{

    private readonly IMailBoxRepository _repository;

    public GetMailBoxByAuthNameHandler(IMailBoxRepository repository)
    {
        _repository = repository;
    }

    public async Task<MailBox?> Handle(GetMailBoxByAuthName request, CancellationToken cancellationToken) =>
        await _repository.GetAsync(box => box.AuthName == request.AuthName);
}