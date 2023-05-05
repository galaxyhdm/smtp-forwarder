using System.Globalization;

namespace SmtpForwarder.Application.Utils;

public class ProcessTraceBucket
{
    private const bool Log = false;
    
    private readonly List<ProcessTrace> _processTraces = new();

    public void LogTrace(string processIdentifier, TraceLevel level, string step, string processCode, string message,
        bool end = false)
    {
        if(!Log) return;
        _processTraces.Add(new ProcessTrace(processIdentifier, level, step, message, processCode, DateTime.UtcNow,
            end));
    }

    /// <summary>
    /// Gets all ProcessTraces with the given processIdentifier
    /// </summary>
    /// <param name="processIdentifier"></param>
    /// <returns>A list with processTraces</returns>
    public List<ProcessTrace> GetTraces(string processIdentifier) =>
        _processTraces
            .Where(trace => trace.ProcessIdentifier.Equals(processIdentifier))
            .OrderBy(trace => trace.TraceTime)
            .ToList();

    /// <summary>
    /// Removes all ProcessTraces with the given processIdentifier
    /// </summary>
    /// <param name="processIdentifier"></param>
    public void RemoveTraces(string processIdentifier) =>
        _processTraces.RemoveAll(trace => trace.ProcessIdentifier.Equals(processIdentifier));

    /// <summary>
    /// Returns a Dictionary with all ended process traces.
    /// Key: The ProcessIdentifier
    /// Value: A List with all ProcessTraces with the same ProcessIdentifier 
    /// </summary>
    /// <returns>A Dictionary with the ProcessIdentifier as key and a list with ProcessTraces</returns>
    public Dictionary<string, IEnumerable<ProcessTrace>> GetEndedTraces()
    {
        var endedTraces = _processTraces.Where(trace => trace.IsEnd).Select(trace => trace.ProcessIdentifier).ToList();
        var dictionary = _processTraces
            .Where(trace => endedTraces.Contains(trace.ProcessIdentifier))
            .OrderByDescending(trace => trace.TraceTime)
            .GroupBy(trace => trace.ProcessIdentifier)
            .ToDictionary(traces => traces.Key, traces => traces.AsEnumerable());

        return dictionary;
    }

    // --------------------------
    // Static
    
    private static ProcessTraceBucket? _bucket;

    public static ProcessTraceBucket Get => _bucket ??= new ProcessTraceBucket();

}

public class ProcessTrace
{
    /// <summary>
    /// A unique identifier that is the same for the hole process from start to end.
    /// Like a messageId or a unique job id. 
    /// </summary>
    public string ProcessIdentifier { get; }

    /// <summary>
    /// Trace time in utc
    /// </summary>
    public DateTime TraceTime { get; }

    public TraceLevel Level { get; }

    /// <summary>
    /// A unique code to identify the corresponding step in the source code 
    /// </summary>
    public string ProcessCode { get; }

    /// <summary>
    /// A short step name to identify a existing sub-step
    /// Like a pre, post, etc.
    /// </summary>
    public string Step { get; }

    /// <summary>
    /// A descriptive human-readable message for the info/error/debug, that normal users can understand  
    /// </summary>
    public string Message { get; }

    public bool IsEnd { get; }

    public ProcessTrace(string processIdentifier, TraceLevel level, string step, string message, string processCode,
        DateTime traceTime, bool end)
    {
        Level = level;
        Step = step;
        Message = message;
        ProcessCode = processCode;
        TraceTime = traceTime;
        IsEnd = end;
        ProcessIdentifier = processIdentifier;
    }

    public override string ToString()
    {
        return
            $"{TraceTime.ToString(CultureInfo.CurrentCulture)} - {Level}: {ProcessIdentifier}#{Step}#{ProcessCode}: {Message} | {IsEnd}";
    }

}

public enum TraceLevel
{
    Debug = 1,
    Info = 2,
    Warn = 3,
    Error = 4
}