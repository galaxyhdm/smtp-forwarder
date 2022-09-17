using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Interfaces.Repositories;

public interface IForwardTargetRepository : IRepository<ForwardTarget>
{
    Task<IEnumerable<ForwardTarget>> GetByForwarderName(string forwarderName);
}