using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    public Task<User?> GetUserAsync(string username);
}