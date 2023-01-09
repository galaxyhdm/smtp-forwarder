using System.Runtime.Loader;

namespace SmtpForwarder.Application.Forwarders;

internal class ForwarderAssemblyLoadContext : AssemblyLoadContext
{
    private readonly string _forwarderFolder;

    public ForwarderAssemblyLoadContext(string forwarderFolder) : base(true)
    {
        _forwarderFolder = forwarderFolder;
    }

}