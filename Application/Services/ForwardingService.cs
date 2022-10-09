using System.Threading.Tasks.Dataflow;
using NLog;
using Polly;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Jobs;

namespace SmtpForwarder.Application.Services;

public class ForwardingService : IForwardingService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly ActionBlock<ForwardingRequest> _actionBlock;
    
    public ForwardingService()
    {
        var policy = Policy
            .HandleInner<NullReferenceException>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(5));
        
        _actionBlock = new ActionBlock<ForwardingRequest>(async job =>
        {
            try
            {
                await policy.ExecuteAsync(job.Run);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while processing request ({})", job.RequestId);
            }
        });
    }

    public void EnqueueForwardingRequest(ForwardingRequest request)
    {
        _actionBlock.Post(request);
    }
}