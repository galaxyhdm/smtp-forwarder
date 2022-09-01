using Domain;

namespace Application.Interfaces.Repositories;

public interface IMailBoxRepository : IRepository<MailBox>
{
    public Task<MailBox?> GetMailBoxAsync(string mailAddress);
}