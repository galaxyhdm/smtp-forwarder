using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

internal sealed class UserRepository : RootRepositoryBase<User>, IUserRepository
{

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger) : base(context)
    {
        Entities = context.Set<User>().AsQueryable();

        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogDebug("Saving user {} - {}",
            args.Entity.UserId,
            args.Entity.Username);
    }

    public async Task<User?> GetUserAsync(string username)
    {
        return await GetAsync(user => user.Username == username);
    }
}