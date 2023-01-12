using Newtonsoft.Json;
using NLog;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Domain;
using SmtpForwarder.ForwardingApi;

namespace SmtpForwarder.Application.Forwarders;

internal class ForwardingController : IForwardingController
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private const string ForwarderPath = "./forwarder";

    private readonly IForwardTargetRepository _repository;
    private readonly Dictionary<string, Type> _forwarderTypes;
    private readonly Dictionary<string, ForwarderFile> _files;
    private readonly Dictionary<Guid, IForwarder> _forwarders;

    public ForwardingController(IForwardTargetRepository repository)
    {
        _repository = repository;
        _forwarderTypes = new Dictionary<string, Type>();
        _files = new Dictionary<string, ForwarderFile>();
        _forwarders = new Dictionary<Guid, IForwarder>();

        if (!Directory.Exists(ForwarderPath))
            Directory.CreateDirectory(ForwarderPath);

        LoadAllForwarderTypes();
        Task.Run(InitializeForwardTargets);
    }

    public IForwarder GetForwarder(Guid id)
    {
        return _forwarders[id];
    }
    
    private async Task InitializeForwardTargetAsync(ForwardTarget target)
    {
        if (!_forwarderTypes.TryGetValue(target.ForwarderName, out var classType))
        {
            Log.Warn("Could not load forward-target ({}), because no matching type found ({})",
                target.Id,
                target.ForwarderName);
            return;
        }

        if (_forwarders.ContainsKey(target.Id))
        {
            Log.Warn("Forward-Taget ({}), already exists!", target.Id);
            return;
        }

        if (Activator.CreateInstance(classType) is not IForwarder forwarder) return;

        _forwarders.Add(target.Id, forwarder);
        await forwarder.InitializeAsync(target.ForwarderSettings);
        Log.Debug("Forward-Target ({}) loaded with forwarder: {}", target.Id, forwarder.Name);
    }

    private async Task InitializeForwardTargets()
    {
        var forwardTargets = await _repository.GetAllAsync(target => target.Enabled);
        foreach (var forwardTarget in forwardTargets)
        {
            try
            {
                await InitializeForwardTargetAsync(forwardTarget);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SMTP-Forwarder failed to load Forward-Target ({})", 
                    forwardTarget.Id);
            }
        }
    }

    private bool LoadForwarderTypes(string forwarderFolder)
    {
        var forwarderFile = new ForwarderFile(forwarderFolder);
        if (!_files.TryAdd(forwarderFile.Filename, forwarderFile))
        {
            Log.Error("Forwarding file already exists: {}", forwarderFile.Filename);
            return false;
        }

        var containingForwarders = forwarderFile.GetContainingForwarders();

        foreach (var keyValuePair in containingForwarders.Where(keyValuePair =>
                     !_forwarderTypes.TryAdd(keyValuePair.Key, keyValuePair.Value)))
        {
            Log.Warn("Forwarder ({}) with name '{}' already registered",
                keyValuePair.Value.FullName,
                keyValuePair.Key);
        }

        return true;
    }

    private void LoadAllForwarderTypes()
    {
        foreach (var directory in Directory.GetDirectories(ForwarderPath))
        {
            try
            {
                var loaded = LoadForwarderTypes(directory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SMTP-Forwarder failed to load forwarder-types in path: {}", directory);
            }
        }

        foreach (var (key, value) in _forwarderTypes)
            Log.Debug("Forwarder-Type ({}), loaded with name: {}", value.FullName, key);
    }

}