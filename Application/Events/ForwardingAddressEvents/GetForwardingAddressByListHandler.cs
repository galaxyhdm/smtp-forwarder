using System.Linq.Expressions;
using MediatR;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.ForwardingAddressEvents;

public record GetForwardingAddressByList
    (List<string> LocalPart, bool OnlyEnabled = true) : IRequest<IEnumerable<ForwardingAddress>>;

public class
    GetForwardingAddressByListHandler : IRequestHandler<GetForwardingAddressByList, IEnumerable<ForwardingAddress>>
{

    private readonly IForwardingAddressRepository _repository;

    public GetForwardingAddressByListHandler(IForwardingAddressRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ForwardingAddress>> Handle(GetForwardingAddressByList request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetByLocalParts(request.LocalPart, request.OnlyEnabled);
    }
}