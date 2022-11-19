using MediatR;
using SmtpForwarder.Domain;

namespace SmtpForwarder.Application.Events.PermissionEvents;

public record ForwardingAddressPermissionCheck(User? User, ForwardingAddress ForwardingAddress) : IRequest<bool>;

public class ForwardingAddressPermissionHandler : IRequestHandler<ForwardingAddressPermissionCheck, bool>
{

    public Task<bool> Handle(ForwardingAddressPermissionCheck request, CancellationToken cancellationToken)
    {
        if(request.User is null) return Task.FromResult(false);
        if(request.ForwardingAddress.OwnerId is null) return Task.FromResult(false);
        return Task.FromResult(request.User.UserId.Equals(request.ForwardingAddress.OwnerId));
    }
}