using MediatR;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.ForwardingAddressEvents;

public record GetForwardingAddressByLocalPart(string LocalPart, bool OnlyEnabled = true) : IRequest<ForwardingAddress?>;

public class GetForwardingAddressHandler : IRequestHandler<GetForwardingAddressByLocalPart, ForwardingAddress?>
{
    private readonly IForwardingAddressRepository _repository;

    public GetForwardingAddressHandler(IForwardingAddressRepository repository)
    {
        _repository = repository;
    }

    public async Task<ForwardingAddress?> Handle(GetForwardingAddressByLocalPart request,
        CancellationToken cancellationToken) =>
        await _repository.GetAsync(address => (address.DeleteTimeUtc == null || address.DeleteTimeUtc < DateTime.UtcNow)
                                        && address.LocalAddressPart.Equals(request.LocalPart)
                                        && (!request.OnlyEnabled || address.Enabled));
}