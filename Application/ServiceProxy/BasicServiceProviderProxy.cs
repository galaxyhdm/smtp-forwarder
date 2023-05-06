using Microsoft.Extensions.DependencyInjection;

namespace SmtpForwarder.Application.ServiceProxy;

public class BasicServiceProviderProxy : IServiceProviderProxy
{

    private readonly IServiceProvider _serviceProvider;

    public BasicServiceProviderProxy(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T? GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }

    public IEnumerable<T> GetServices<T>()
    {
        return _serviceProvider.GetServices<T>();
    }

    public object? GetService(Type type)
    {
        return _serviceProvider.GetService(type);
    }

    public IEnumerable<object?> GetServices(Type type)
    {
        return _serviceProvider.GetServices(type);
    }
}