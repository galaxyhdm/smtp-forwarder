using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using SmtpReceiverServer.Authorization;
using SmtpReceiverServer.Handlers;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpReceiverServer;

public static class SmtpInjector
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static IServiceCollection AddSmtpService(this IServiceCollection collection)
    {
        collection
            .RegisterSmtpServer()
            .AddHostedService<SmtpService>();

        return collection;
    }

    private static IServiceCollection RegisterSmtpServer(this IServiceCollection collection)
    {
        collection.AddTransient<IMailboxFilter, IncomingMailboxFilter>();
        collection.AddTransient<IUserAuthenticator, SmtpUserAuthenticator>();
        collection.AddTransient<IMessageStore, IncomingMailHandler>();

        var options = new SmtpServerOptionsBuilder()
            .ServerName("SmtpForwarder")
            .Endpoint(endpointBuilder =>
                endpointBuilder
                    .Port(3456)
                    .Certificate(null)
                    .AuthenticationRequired()
                    .AllowUnsecureAuthentication(true)
                    .ReadTimeout(TimeSpan.FromSeconds(30))
            ).Build();

        collection.AddSingleton(provider => new SmtpServer.SmtpServer(
            options,
            provider.GetRequiredService<IServiceProvider>()));
        
        return collection;
    }
}