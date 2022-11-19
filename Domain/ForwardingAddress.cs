namespace SmtpForwarder.Domain;

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

    private ForwardingAddress()
    {
    }
    
    private void SetLocalAddressPart(string localAddressPart)
    {
        if(string.IsNullOrWhiteSpace(localAddressPart))
            throw new ArgumentException("Must have a value.", nameof(localAddressPart));

        LocalAddressPart = localAddressPart;
    }
    
    public void SetEnabled(bool enabled) =>
        Enabled = enabled;
    
    public void SetDeleteTime(DateTime? dateTime = null)
    {
        DeleteTimeUtc = dateTime ?? DateTime.UtcNow;
    }

    public void LinkWithOwner(User owner) =>
        Owner = owner;

    public void LinkWithTarget(ForwardTarget? forwardTarget) =>
        ForwardTarget = forwardTarget;

    public bool IsDeleted() =>
        DeleteTimeUtc is not null && DeleteTimeUtc < DateTime.UtcNow;
}