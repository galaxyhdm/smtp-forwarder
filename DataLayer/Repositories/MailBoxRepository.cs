using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLayer.Repositories;

internal sealed class MailBoxRepository : RootRepositoryBase<MailBox>, IMailBoxRepository
{
    
    public MailBoxRepository(AppDbContext context, ILogger<UserRepository> logger) : base(context)
    {
        Entities = context.Set<MailBox>()
            .Include(m => m.Owner)
            .AsQueryable();

        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogDebug("Saving mailbox {} - {}",
            args.Entity.MailBoxId,
            args.Entity.MailAddress);
    }

    public async Task<MailBox?> GetMailBoxAsync(string mailAddress) =>
        await GetAsync(m => m.MailAddress == mailAddress);

}