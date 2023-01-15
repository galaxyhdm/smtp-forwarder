using System.Net;
using System.Threading.RateLimiting;
using MediatR;
using NLog;
using SmtpForwarder.Application;
using SmtpForwarder.Application.Events.AuthorizationEvents;
using SmtpForwarder.Application.Events.MailBoxEvents;
using SmtpForwarder.SmtpReceiverServer.Extensions;
using SmtpServer;
using SmtpServer.Authentication;

namespace SmtpForwarder.SmtpReceiverServer.Authorization;

internal class SmtpUserAuthenticator : IUserAuthenticator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;
    
    private static readonly PartitionedRateLimiter<IPEndPoint> RateLimiter = PartitionedRateLimiter.Create<IPEndPoint, string>(ipEndPoint =>
        RateLimitPartition.GetFixedWindowLimiter(ipEndPoint.Address.ToString(), _ => new()
        {
            AutoReplenishment = true,
            PermitLimit = 20,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            Window = TimeSpan.FromMinutes(10)
        }));
    
    public SmtpUserAuthenticator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<bool> AuthenticateAsync(ISessionContext context, string user, string password,
        CancellationToken cancellationToken)
    {
        using var lease = await RateLimiter.AcquireAsync(context.GetIp(), 1, cancellationToken);
        var rateLimiterStatistics = RateLimiter.GetStatistics(context.GetIp());
        if (!lease.IsAcquired)
        {
            Log.Info("New incomming smtp request from {}, blocked!", context.GetIpString()); 
            await Task.Delay(TimeSpan.FromMilliseconds(5210), cancellationToken);
            return false;
        }

        Log.Info("New incomming smtp request from: {}", context.GetIpString());
        Log.Debug("Starting mailbox authentication. ({})", user);
        //return false;
        context.Properties.Add(Constants.SessionStartKey, DateTime.UtcNow);

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