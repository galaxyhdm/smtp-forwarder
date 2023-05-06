namespace SmtpForwarder.Application.ServiceProxy;

public static class ServiceLocator
{
    private static IServiceProviderProxy? _providerProxy;

    public static IServiceProviderProxy ServiceProvider => _providerProxy ??
                                                            throw new Exception(
                                                                "You should Initialize the ServiceProvider before using it.");

    public static void Initialize(IServiceProviderProxy proxy) =>
        _providerProxy = proxy;
}