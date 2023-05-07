using SmtpForwarder.Domain.Enums;

namespace SmtpForwarder.Domain;

public class TraceLogEntry : EntityBase
{

    public Guid Id { get; }

    public DateTime TraceTime { get; }

    public TraceLevel TraceLevel { get; }

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

    private TraceLogEntry(Guid id, DateTime traceTime, TraceLevel traceLevel, string processCode, string step,
        string message, bool isEnd, Guid traceLogId)
    {
        Id = id;
        TraceTime = traceTime;
        ProcessCode = processCode;
        Step = step;
        Message = message;
        IsEnd = isEnd;
        TraceLevel = traceLevel;

        LinkWithTraceLog(traceLogId);
    }

    public static TraceLogEntry CreateTraceLogEntry(DateTime traceTime, TraceLevel traceLevel, string processCode,
        string step, string message, bool isEnd, Guid traceLogId)
    {
        var id = Guid.NewGuid();
        return new TraceLogEntry(id, traceTime, traceLevel, processCode, step, message, isEnd, traceLogId);
    }

    private void LinkWithTraceLog(Guid traceLogId) =>
        TraceLogId = traceLogId;

}