namespace SmtpForwarder.Domain;

public class ForwardTarget : EntityBase
{
    
    public Guid Id { get; }
    public string Name { get; private set; }
    public string ForwarderName { get; private set; }
    public string ForwarderSettings { get; private set; }
    public bool Enabled { get; private set; }
    
    // ------------------------------
    // Relationships
    public Guid OwnerId { get; }
    public User Owner { get; }

    public IReadOnlyCollection<ForwardingAddress> ForwardingAddresses => _forwardingAddresses; 
    private readonly List<ForwardingAddress> _forwardingAddresses;

    private ForwardTarget()
    {
        _forwardingAddresses = new List<ForwardingAddress>();
    }

    public ForwardTarget(Guid id, string name, string forwarderName, string forwarderSettings, User owner)
    {
        Id = id;
        Enabled = true;
        Owner = owner;
        UpdateName(name);
        UpdateForwarder(forwarderName, forwarderSettings);
    }

    public static ForwardTarget CreateForwardTarget(string name, string forwarderName, string forwarderSettings,
        User owner)
    {
        var id = Guid.NewGuid();
        var forwardTarget = new ForwardTarget(id, name, forwarderName, forwarderSettings, owner);
        return forwardTarget;
    }
    
    public void UpdateName(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Must have a value.", nameof(name));
        Name = name;
    }
    
    public void UpdateForwarder(string forwarderName, string settings)
    {
        if(string.IsNullOrWhiteSpace(forwarderName))
            throw new ArgumentException("Must have a value.", nameof(forwarderName));
        ForwarderName = forwarderName;
        UpdateForwarderSettings(settings);
    }

    public void UpdateForwarderSettings(string settings)
    {
        if(string.IsNullOrWhiteSpace(settings))
            throw new ArgumentException("Must have a value.", nameof(settings));
        ForwarderSettings = settings;
    }
 
    public void SetEnabled(bool enabled) =>
        Enabled = enabled;
    
}