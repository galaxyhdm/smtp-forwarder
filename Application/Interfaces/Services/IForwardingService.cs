using SmtpForwarder.Application.Jobs;

namespace SmtpForwarder.Application.Interfaces.Services;

public interface IForwardingService
{
    void EnqueueForwardingRequest(ForwardingRequest request);
}