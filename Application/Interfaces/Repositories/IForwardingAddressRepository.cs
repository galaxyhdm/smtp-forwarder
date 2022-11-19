using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Interfaces.Repositories;

public interface IForwardingAddressRepository : IRepository<ForwardingAddress>
{
    Task<IEnumerable<ForwardingAddress>> GetByLocalParts(List<string> localParts, bool onlyEnabled = true);
}