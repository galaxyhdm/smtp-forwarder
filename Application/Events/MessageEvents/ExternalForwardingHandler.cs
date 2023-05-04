using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.Extensions.Options;
using MimeKit;
using NLog;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain.Settings;
using TraceLevel = SmtpForwarder.Application.Utils.TraceLevel;

namespace SmtpForwarder.Application.Events.MessageEvents;

public record ExternalForwardingRequest
    (Guid RequestId, MimeMessage Message, List<MailboxAddress> Addresses) : IRequest<bool>;

public class ExternalForwardingHandler : IRequestHandler<ExternalForwardingRequest, bool>
{

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly MailSettings _mailSettings;
    private readonly string _internalDomainPart;

    private readonly IMediator _mediator;

    public ExternalForwardingHandler(IMediator mediator, IOptions<Settings> settingOptions)
    {
        _mediator = mediator;
        _internalDomainPart = settingOptions.Value.InternalDomain;
        _mailSettings = settingOptions.Value.MailSettings;
    }

    public async Task<bool> Handle(ExternalForwardingRequest request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var requestId, out var message, out var addresses);

        Log.Trace("========== Starting external forward ========");
        if (addresses.Count == 0) return false;
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var mail = new MimeMessage();

        var fromAddress = message.From.FirstOrDefault() ??
                          new MailboxAddress(_mailSettings.DisplayName, _mailSettings.From);

        mail.From.Add(fromAddress);
        mail.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.From);
        mail.ReplyTo.Add(mail.Sender);

        //Set BCC to addresses
        addresses.ForEach(address => mail.Bcc.Add(address));

        mail.Subject = message.Subject;
        mail.Body = message.Body;


        using var smtp = new SmtpClient();

        if (_mailSettings.UseSSL)
        {
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect,
                cancellationToken);
        }
        else if (_mailSettings.UseStartTls)
        {
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls,
                cancellationToken);
        }

        await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password, cancellationToken);
        await smtp.SendAsync(mail, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);

        stopwatch.Stop();
        var milliseconds = stopwatch.Elapsed.TotalMilliseconds;

        Log.Debug("Finished external forwarding for mail: {} ({}) in {}ms", message.MessageId, requestId, milliseconds);
        
        ProcessTraceBucket.Get.LogTrace(message.MessageId, TraceLevel.Info, "forwarding-external", 
            "forward-external-1",
            "Email was forwarded with an external SMTP server.");
        
        return true;
    }
}