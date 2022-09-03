namespace Domain;

public class ForwardTarget : EntityBase
{
    
    public Guid Id { get; }
    public string Name { get; }
    public string ForwarderName { get; private set; }
    public string ForwarderSettings { get; private set; }
    public bool Enabled { get; private set; }
    
    // ------------------------------
    // Relationships
    public Guid OwnerId { get; }
    public User Owner { get; private set; }

    public IReadOnlyCollection<ForwardingAddress> ForwardingAddresses => _forwardingAddresses; 
    private readonly List<ForwardingAddress> _forwardingAddresses;

    public ForwardTarget()
    {
        _forwardingAddresses = new List<ForwardingAddress>();
    }
}