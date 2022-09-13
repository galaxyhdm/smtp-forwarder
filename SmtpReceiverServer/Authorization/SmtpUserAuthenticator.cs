using Application.Events.AuthorizationEvents;
using Application.Events.MailBoxEvents;
using MediatR;
using NLog;
using SmtpServer;
using SmtpServer.Authentication;

namespace SmtpReceiverServer.Authorization;

internal class SmtpUserAuthenticator : IUserAuthenticator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;

    public SmtpUserAuthenticator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
    {
        Log.Debug("Starting mailbox authentication. ({})", user);

        var mailBox = await _mediator.Send(new GetMailBoxByAuthName(user), cancellationToken);
        if (mailBox == null)
        {
            Log.Debug("Mailbox not found. ({})", user);
            await _mediator.Send(new GetPasswordHash(""), cancellationToken);
            return false;
        }

        var auth = await _mediator.Send(new ValidatePassword(password, mailBox.PasswordHash), cancellationToken);
        
        Log.Debug($"Auth request finished. ({user} | {mailBox.MailAddress} | {auth.ToString().ToLower()})");
        //if (auth) context.Properties.Add(Constants.InternalMailBoxKey, mailBox);
        return auth;
    }
}