using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

internal sealed class TraceLogEntryRepository: RootRepositoryBase<TraceLogEntry>, ITraceLogEntryRepository
{

    public TraceLogEntryRepository(AppDbContext context, ILogger<TraceLogEntryRepository> logger) : 
        base(context)
    {
        Entities = context.Set<TraceLogEntry>()
            .Include(entry => entry.TraceLog)
            .AsQueryable();
        
        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogTrace("Saving trace_log_entry {}",
            args.Entity.Id);
    }
}