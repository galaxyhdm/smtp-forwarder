using Domain;

namespace Application.Interfaces.Repositories;

public interface IForwardTargetRepository : IRepository<ForwardTarget>
{
    Task<IEnumerable<ForwardTarget>> GetByForwarderName(string forwarderName);
}