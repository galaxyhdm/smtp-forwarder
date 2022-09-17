using MediatR;
using SmtpForwarder.Application.Events.AuthorizationEvents;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.MailBoxEvents;

public record CreateMailBox(string MailAddress, string AuthName, string Password, Guid? UserId) : IRequest<MailBox?>;

public class CreateMailBoxHandler : IRequestHandler<CreateMailBox, MailBox?>
{

    private readonly IMediator _mediator;
    private readonly IMailBoxRepository _repository;

    public CreateMailBoxHandler(IMediator mediator, IMailBoxRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    public async Task<MailBox?> Handle(CreateMailBox request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out string mailAddress, out string authName, out string password, out Guid? user);

        var passwordHash = await _mediator.Send(new GetPasswordHash(password), cancellationToken);

        var mailBox = MailBox.CreateMailBox(mailAddress, authName, passwordHash, null);

        var success = await _repository.AddAsync(mailBox);
        return success ? mailBox : null;
    }
}