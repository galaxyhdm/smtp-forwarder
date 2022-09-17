namespace SmtpForwarder.Domain;

public class User : EntityBase
{

    public Guid UserId { get; }
    public string Username { get; private set; }
    public string? DisplayName { get; private set; }

    public bool IsAdmin { get; private set; }
    public byte[] PasswordHash { get; private set; }

    public User()
    {
    }

    private User(Guid userId, string username, string? displayName, bool isAdmin, byte[] passwordHash)
    {
        UserId = userId;
        SetUsername(username);
        SetDisplayName(displayName);
        UpdateAdminState(isAdmin);
        UpdatePasswordHash(passwordHash);
    }

    public static User CreateUser(string username, bool isAdmin, byte[] passwordHash, string? displayName = null)
    {
        var id = Guid.NewGuid();
        var user = new User(id, username, displayName, isAdmin, passwordHash);
        return user;
    }
    
    private void SetUsername(string username)
    {
        if(string.IsNullOrWhiteSpace(username)) 
            throw new ArgumentException("Must have a value.", nameof(username));

        Username = username;
    }

    public void SetDisplayName(string? displayName)
    {
        if(displayName == null)
            RemoveDisplayName();
        else
            UpdateDisplayName(displayName);
    }
    
    public void RemoveDisplayName() =>
        DisplayName = null;
    
    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Must have a value.", nameof(displayName));
        DisplayName = displayName;
    }

    public void UpdateAdminState(bool admin) =>
        IsAdmin = admin;

    public void UpdatePasswordHash(byte[] passwordHash) =>
        PasswordHash = passwordHash;

}