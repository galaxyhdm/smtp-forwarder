using Microsoft.Extensions.DependencyInjection;
using NLog;
using SmtpForwarder.Application.Interfaces.Security;
using SmtpForwarder.SmtpReceiverServer.Authorization;
using SmtpForwarder.SmtpReceiverServer.Handlers;
using SmtpForwarder.SmtpReceiverServer.Security;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.Storage;

namespace SmtpForwarder.SmtpReceiverServer;

public static class SmtpInjector
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly ICertificateProvider CertificateProvider = new CertificateProvider("tls");
    
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

        var x509Certificate = CertificateProvider.GetCertificate(out var unsecureAuthentication);
        
        if (x509Certificate == null && !unsecureAuthentication)
            Log.Warn(
                "Could not found or generate certificate please make sure to use the correct env's. Booting-up, but not able to accept smtp login data.");
        if (x509Certificate == null && unsecureAuthentication)
            Log.Warn("Starting without tls-encryption! Not recommended for using in production!");
        if (x509Certificate != null && unsecureAuthentication)
            Log.Warn("Found certificate, but 'unsecureAuthentication' is allowed, is this okay?");

        
        var options = new SmtpServerOptionsBuilder()
            .ServerName("SmtpForwarder")
            .Endpoint(endpointBuilder =>
                endpointBuilder
                    .Port(3456)
                    .Certificate(x509Certificate)
                    .AuthenticationRequired()
                    .AllowUnsecureAuthentication(unsecureAuthentication)
                    .ReadTimeout(TimeSpan.FromSeconds(30))
            ).Build();

        collection.AddSingleton(provider => new SmtpServer.SmtpServer(
            options,
            provider.GetRequiredService<IServiceProvider>()));
        
        return collection;
    }
}