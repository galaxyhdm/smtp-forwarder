using MediatR;
using NLog;
using SmtpForwarder.Application;
using SmtpForwarder.Application.Events.AuthorizationEvents;
using SmtpForwarder.Application.Events.MailBoxEvents;
using SmtpServer;
using SmtpServer.Authentication;

namespace SmtpForwarder.SmtpReceiverServer.Authorization;

internal class SmtpUserAuthenticator : IUserAuthenticator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public SmtpUserAuthenticator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> AuthenticateAsync(ISessionContext context, string user, string password,
        CancellationToken cancellationToken)
    {
        Log.Debug("Starting mailbox authentication. ({})", user);

        //await _mediator.Send(new CreateMailBox("test", "auth1234", "auth1234", null));

        var mailBox = await _mediator.Send(new GetMailBoxByAuthName(user), cancellationToken);
        if (mailBox is not {Enabled: true})
        {
            Log.Debug("Mailbox not found. ({})", user);
            await _mediator.Send(new GetPasswordHash(""), cancellationToken);
            return false;
        }

        var auth = await _mediator.Send(new ValidatePassword(password, mailBox.PasswordHash), cancellationToken);

        Log.Debug($"Auth request finished. ({user} | {mailBox.MailAddress} | {auth.ToString().ToLower()})");
        if (auth) context.Properties.Add(Constants.InternalMailBoxKey, mailBox);
        return auth;
    }
}