namespace SmtpForwarder.Domain;

internal interface IEntityBase
{
    DateTime CreatedUtc { get; }
    DateTime LastUpdatedUtc { get; }
    bool NotUpdatedYet { get; }
    bool NotInserted { get; }

    void LogCreatedUpdated(bool created = false);
}

public class EntityBase : IEntityBase
{

    public DateTime CreatedUtc { get; private set; }

    public DateTime LastUpdatedUtc { get; private set; }

    public bool NotUpdatedYet => CreatedUtc == LastUpdatedUtc;

    public bool NotInserted => CreatedUtc == default && LastUpdatedUtc == default;

    public EntityBase()
    {
    }

    public void LogCreatedUpdated(bool created = false)
    {
        var timeNow = DateTime.UtcNow;
        if (created)
            CreatedUtc = timeNow;
        LastUpdatedUtc = timeNow;
    }
}