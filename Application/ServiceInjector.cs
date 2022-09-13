using System.Reflection;
using Application.Authorization;
using Application.Interfaces.Authorization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceInjector
{
    public static IServiceCollection AddEvents(this IServiceCollection serviceCollection, Assembly assembly)
    {
        serviceCollection.AddMediatR(assembly);
        return serviceCollection;
    }
    
    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services) {
        services.AddSingleton<IPasswordHasher, Argon2Hasher>();
        //services.AddSingleton<IAuthTokenGenerator, AuthTokenGenerator>();
        return services;
    }
}