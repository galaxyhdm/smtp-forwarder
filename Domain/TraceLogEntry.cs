namespace SmtpForwarder.Domain;

public class TraceLogEntry : EntityBase
{

    public Guid Id { get; }

    public DateTime TraceTime { get; }
    //TODO: Set TraceLevel (move to Domain-Project) public

    public string ProcessCode { get; }
    public string Step { get; }
    public string Message { get; }
    public bool IsEnd { get; }

    // ------------------------------
    // Relationships
    public Guid TraceLogId { get; private set; }
    public TraceLog TraceLog { get; }

    public TraceLogEntry()
    {
    }

    private TraceLogEntry(Guid id, DateTime traceTime, string processCode, string step, string message, bool isEnd,
        Guid traceLogId)
    {
        Id = id;
        TraceTime = traceTime;
        ProcessCode = processCode;
        Step = step;
        Message = message;
        IsEnd = isEnd;

        LinkWithTraceLog(traceLogId);
    }

    public static TraceLogEntry CreateTraceLogEntry(DateTime traceTime, string processCode, string step, string message,
        bool isEnd, Guid traceLogId)
    {
        var id = Guid.NewGuid();
        return new TraceLogEntry(id, traceTime, processCode, step, message, isEnd, traceLogId);
    }

    private void LinkWithTraceLog(Guid traceLogId) =>
        TraceLogId = traceLogId;

}