namespace SmtpForwarder.Domain;

public class MailBox : EntityBase
{

    public Guid MailBoxId { get; }
    public string LocalAddressPart { get; private set; }
    public string AuthName { get; private set; }
    public byte[] PasswordHash { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime? DeleteTimeUtc { get; private set; }

    // ------------------------------
    // Relationships
    public Guid? OwnerId { get; }
    public User? Owner { get; private set; }

    //todo: set host part over evn or config
    public string MailAddress => $"{LocalAddressPart}@test.lab";
    
    public MailBox()
    {
    }

    private MailBox(Guid mailBoxId, string localAddressPart, string authName, byte[] passwordHash, User? owner)
    {
        MailBoxId = mailBoxId;
        Enabled = true;
        
        SetMailAddress(localAddressPart);
        UpdateAuthName(authName);
        UpdatePasswordHash(passwordHash);
        LinkWithUser(owner);
    }

    public static MailBox CreateMailBox(string localAddressPart, string authName, byte[] passwordHash, User? user)
    {
        var id = Guid.NewGuid();
        var mailBox = new MailBox(id, localAddressPart, authName, passwordHash, user);
        return mailBox;
    }
    
    private void SetMailAddress(string mailAddress)
    {
        if(string.IsNullOrWhiteSpace(mailAddress))
            throw new ArgumentException("Must have a value.", nameof(mailAddress));

        LocalAddressPart = mailAddress;
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

    private void LinkWithUser(User? owner) =>
        Owner = owner;

}