using MediatR;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.TraceLogEvents;

public record SaveTraceLogRequest
    (string ProcessIdentifier, List<ProcessTrace> ProcessTraces, MailBox? MailBox) : IRequest<bool>;

public class SaveTraceLogHandler : IRequestHandler<SaveTraceLogRequest, bool>
{
    private readonly string _version;

    private readonly ITraceLogRepository _traceLogRepository;
    private readonly ITraceLogEntryRepository _traceLogEntryRepository;

    public SaveTraceLogHandler(ITraceLogRepository traceLogRepository, ITraceLogEntryRepository traceLogEntryRepository)
    {
        _version = GetType().Assembly.GetName().Version?.ToString() ?? "undefined";
        _traceLogRepository = traceLogRepository;
        _traceLogEntryRepository = traceLogEntryRepository;
    }

    public async Task<bool> Handle(SaveTraceLogRequest request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var processIdentifier, out var processTraces, out var mailBox);

        if (processTraces.Count == 0) return false;

        var traceLog = TraceLog.CreateTraceLog(processIdentifier,
            _version,
            processTraces.Min(trace => trace.TraceTime),
            processTraces.Max(trace => trace.TraceTime),
            processTraces.Any(trace => trace.IsEnd),
            mailBox?.MailBoxId
        );

        await _traceLogRepository.AddAsync(traceLog);

        foreach (var traceLogEntry in processTraces
                     .Select(processTrace =>
                         TraceLogEntry.CreateTraceLogEntry(processTrace.TraceTime,
                             processTrace.ProcessCode,
                             processTrace.Step,
                             processTrace.Message,
                             processTrace.IsEnd,
                             traceLog.Id)))
        {
            await _traceLogEntryRepository.AddAsync(traceLogEntry);
        }

        return true;
    }
}