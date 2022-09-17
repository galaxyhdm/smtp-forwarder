using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

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
            args.Entity.LocalAddressPart);
    }

    public async Task<MailBox?> GetMailBoxAsync(string localAddressPart) =>
        await GetAsync(m => m.LocalAddressPart == localAddressPart);

}