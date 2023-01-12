using SmtpForwarder.ForwardingApi;

namespace SmtpForwarder.Application.Interfaces.Services;

public interface IForwardingController
{
    IForwarder GetForwarder(Guid id);
}