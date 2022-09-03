namespace Domain;

public class ForwardingAddress : EntityBase
{
    
    public Guid Id { get; }
    public string LocalAddressPart { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime? DeleteTimeUtc { get; private set; }
    
    // ------------------------------
    // Relationships
    public Guid? OwnerId { get; }
    public User? Owner { get; private set; }

    public Guid? ForwardTargetId { get; }
    public ForwardTarget? ForwardTarget { get; private set; }

    public ForwardingAddress()
    {
    }
}