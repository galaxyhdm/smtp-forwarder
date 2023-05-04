using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

internal sealed class TraceLogRepository: RootRepositoryBase<TraceLog>, ITraceLogRepository
{

    public TraceLogRepository(AppDbContext context, ILogger<TraceLogRepository> logger ) : base(context)
    {
        Entities = context.Set<TraceLog>()
            .Include(entry => entry.MailBox)
            .AsQueryable();
        
        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogDebug("Saving trace_entry {}",
            args.Entity.Id);
    }
}