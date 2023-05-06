using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

internal sealed class ForwardTargetRepository : RootRepositoryBase<ForwardTarget>, IForwardTargetRepository
{

    public ForwardTargetRepository(AppDbContext context, ILogger<ForwardTargetRepository> logger) : base(context)
    {
        Entities = context.Set<ForwardTarget>()
            .Include(f => f.Owner)
            .Include(f => f.ForwardingAddresses)
            .AsQueryable();

        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogTrace("Saving forward_target {} - {}",
            args.Entity.Id,
            args.Entity.Name);
    }

    public async Task<IEnumerable<ForwardTarget>> GetByForwarderName(string forwarderName) =>
        await GetAllAsync(target => target.ForwarderName == forwarderName);
}