using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Interfaces.Repositories;

public interface IMailBoxRepository : IRepository<MailBox>
{
    public Task<MailBox?> GetMailBoxAsync(string mailAddress);
}