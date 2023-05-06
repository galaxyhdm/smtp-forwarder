using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SmtpForwarder.Application.Authorization;
using SmtpForwarder.Application.Forwarders;
using SmtpForwarder.Application.Interfaces.Authorization;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.ServiceProxy;
using SmtpForwarder.Application.Services;

namespace SmtpForwarder.Application;

public static class ServiceInjector
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        return serviceCollection;
    }
    
    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services) {
        services.AddSingleton<IPasswordHasher, Argon2Hasher>();
        services.AddSingleton<IIncomingMessageService, IncomingMessageService>();
        services.AddSingleton<IForwardingService, ForwardingService>();
        
        services.AddSingleton<IForwardingController, ForwardingController>();
        //services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        return services;
    }

    public static IServiceCollection AddSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<IServiceProviderProxy, BasicServiceProviderProxy>();

        return services;
    }
    
    public static void WarmUp(this IServiceProvider app)
    {
        app.GetRequiredService<IForwardingController>();
        ServiceLocator.Initialize(app.GetRequiredService<IServiceProviderProxy>());
    } 
}