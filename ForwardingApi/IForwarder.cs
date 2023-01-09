namespace SmtpForwarder.ForwardingApi;

public interface IForwarder
{
    Task InitializeAsync(dynamic forwarderConfig);

    Task ForwardMessage();
    
    string Name { get; }
}