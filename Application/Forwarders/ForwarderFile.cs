using System.Reflection;
using System.Runtime.Loader;
using NLog;
using SmtpForwarder.ForwardingApi;

namespace SmtpForwarder.Application.Forwarders;

public class ForwarderFile
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public string Filename => _filename;

    private readonly string _filename;
    private readonly string _forwarderFolder;

    private readonly ForwarderAssemblyLoadContext _assemblyLoadContext;
    private readonly Dictionary<string, Type> _forwarderTypes;

    public ForwarderFile(string forwarderFolder)
    {
        _forwarderTypes = new Dictionary<string, Type>();
        _forwarderFolder = forwarderFolder;
        _filename = Path.GetFileName(_forwarderFolder);

        _assemblyLoadContext = new ForwarderAssemblyLoadContext(_forwarderFolder);
    }

    public Dictionary<string, Type> GetContainingForwarders()
    {
        _forwarderTypes.Clear();
        var appAssemblies = LoadAssemblies();

        var iForwarder = typeof(IForwarder);

        foreach (var appAssembly in appAssemblies)
        foreach (var classType in appAssembly.ExportedTypes)
        {
            var isForwarder = classType.GetInterfaces().Any(type => type == iForwarder);

            if (!isForwarder) continue;
            var customAttribute = classType.GetCustomAttribute(typeof(ForwardingAttribute));
            if (customAttribute is null)
            {
                Log.Warn("Could not load forwarder ({}), because no forwarding-attribute found!",
                    classType.FullName);
                continue;
            }

            if (customAttribute is not ForwardingAttribute forwardingAttribute) continue;

            if (!_forwarderTypes.TryAdd(forwardingAttribute.Name, classType))
            {
                Log.Warn("Forwarder-Type ({}), not added, because name '{}' already exists!",
                    classType.FullName,
                    forwardingAttribute.Name);
            }
        }

        return _forwarderTypes;
    }

    private IEnumerable<Assembly> LoadAssemblies()
    {
        //load app assemblies
        var loadedAssemblies = AssemblyLoadContext.Default.Assemblies;
        var appAssemblies = new List<Assembly>();

        foreach (var dllFile in Directory.GetFiles(_forwarderFolder, "*.dll", SearchOption.TopDirectoryOnly))
        {
            var dllFileName = Path.GetFileNameWithoutExtension(dllFile);

            var isLoaded = false;

            foreach (var loadedAssembly in loadedAssemblies)
            {
                if(loadedAssembly.IsDynamic) continue;
                if (!string.IsNullOrEmpty(loadedAssembly.Location))
                {
                    if (!Path.GetFileNameWithoutExtension(loadedAssembly.Location)
                            .Equals(dllFileName, StringComparison.OrdinalIgnoreCase)) continue;
                    isLoaded = true;
                    break;
                }

                var assemblyName = loadedAssembly.GetName();

                if (assemblyName.Name == null ||
                    !assemblyName.Name.Equals(dllFileName, StringComparison.OrdinalIgnoreCase)) continue;
                isLoaded = true;
                break;
            }

            if (isLoaded)
                continue;

            try
            {
                var pdbFile = Path.Combine(_forwarderFolder, Path.GetFileNameWithoutExtension(dllFile) + ".pdb");

                using var dllStream = new FileStream(dllFile, FileMode.Open, FileAccess.Read);
                using var pdbStream = File.Exists(pdbFile)
                    ? new FileStream(pdbFile, FileMode.Open, FileAccess.Read)
                    : null;
                appAssemblies.Add(_assemblyLoadContext.LoadFromStream(dllStream, pdbStream));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while loading assablies");
            }
        }

        return appAssemblies;
    }
}