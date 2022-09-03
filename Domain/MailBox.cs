namespace Domain;

public class MailBox : EntityBase
{

    public Guid MailBoxId { get; }
    public string MailAddress { get; private set; }
    public string AuthName { get; private set; }
    public byte[] PasswordHash { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime? DeleteTimeUtc { get; private set; }

    // ------------------------------
    // Relationships
    public Guid? OwnerId { get; }
    public User? Owner { get; private set; }

    public MailBox()
    {
    }

    private MailBox(Guid mailBoxId, string mailAddress, string authName, byte[] passwordHash, User owner)
    {
        MailBoxId = mailBoxId;
        Enabled = true;
        
        SetMailAddress(mailAddress);
        UpdateAuthName(authName);
        UpdatePasswordHash(passwordHash);
        LinkWithUser(owner);
    }

    public static MailBox CreateMailBox(string mailAddress, string authName, byte[] passwordHash, User user)
    {
        var id = Guid.NewGuid();
        var mailBox = new MailBox(id, mailAddress, authName, passwordHash, user);
        return mailBox;
    }
    
    private void SetMailAddress(string mailAddress)
    {
        if(string.IsNullOrWhiteSpace(mailAddress))
            throw new ArgumentException("Must have a value.", nameof(mailAddress));

        MailAddress = mailAddress;
    }
    
    public void UpdateAuthName(string authName)
    {
        if(string.IsNullOrWhiteSpace(authName))
            throw new ArgumentException("Must have a value.", nameof(authName));

        AuthName = authName;
    }
    
    public void UpdatePasswordHash(byte[] passwordHash) =>
        PasswordHash = passwordHash;

    public void SetEnabled(bool enabled) =>
        Enabled = enabled;

    public void SetDeleteTime(DateTime? dateTime = null)
    {
        DeleteTimeUtc = dateTime ?? DateTime.UtcNow;
    }

    private void LinkWithUser(User owner) =>
        Owner = owner;

}