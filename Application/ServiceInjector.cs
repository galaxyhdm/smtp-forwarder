using System.Reflection;
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
}