namespace SmtpForwarder.Domain;

public class TraceLog : EntityBase
{
    public Guid Id { get; }

    public string ProcessIdentifier { get; }
    public string ApplicationVersion { get; }

    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public bool Ended { get; }

    // ------------------------------
    // Relationships
    public Guid? MailBoxId { get; private set; }
    public MailBox? MailBox { get; }

    public TraceLog()
    {
    }

    private TraceLog(Guid id, string processIdentifier, string applicationVersion, DateTime startTime, DateTime endTime,
        bool ended, Guid? mailBoxId)
    {
        Id = id;
        ProcessIdentifier = processIdentifier;
        ApplicationVersion = applicationVersion;
        StartTime = startTime;
        EndTime = endTime;
        Ended = ended;
        LinkWithMailBox(mailBoxId);
    }

    public static TraceLog CreateTraceLog(string processIdentifier, string applicationVersion, DateTime startTime,
        DateTime endTime, bool ended, Guid? mailBoxId)
    {
        var id = Guid.NewGuid();

        return new TraceLog(id, processIdentifier, applicationVersion, startTime, endTime, ended, mailBoxId);
    }

    private void LinkWithMailBox(Guid? mailBoxId) =>
        MailBoxId = mailBoxId;
}