using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmtpReceiverServer;

public class SmtpService : BackgroundService
{

    private readonly ILogger<SmtpService> _logger;
    private readonly SmtpServer.SmtpServer _smtpServer;

    public SmtpService(SmtpServer.SmtpServer smtpServer, ILogger<SmtpService> logger)
    {
        _smtpServer = smtpServer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _smtpServer.StartAsync(stoppingToken);
    }

}