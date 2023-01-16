using System.Threading.Tasks.Dataflow;
using MediatR;
using MimeKit;
using NLog;
using Polly;
using SmtpForwarder.Application.Interfaces.Services;
using SmtpForwarder.Application.Jobs;

namespace SmtpForwarder.Application.Services;

public class ForwardingService : IForwardingService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly IMediator _mediator;
    private readonly ActionBlock<ForwardingRequest> _actionBlock;
    
    public ForwardingService(IMediator mediator)
    {
        _mediator = mediator;
        var policy = Policy
            .HandleInner<NullReferenceException>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2));

        var blockOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 2
        };

        _actionBlock = new ActionBlock<ForwardingRequest>(async job =>
        {
            try
            {
                job.SetMediator(_mediator);
                await policy.ExecuteAsync(job.Run);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while processing request ({})", job.RequestId);
                SaveFailedMessage(job);
            }
        }, blockOptions);
    }

    private async void SaveFailedMessage(ForwardingRequest job)
    {
        const string errorDir = "error_messages";
        
        if (!Directory.Exists(errorDir))
            Directory.CreateDirectory(errorDir);
        
        var filePath = Path.Combine(errorDir, $"{job.RequestId}");
        await job.Message.WriteToAsync(FormatOptions.Default, filePath);
        Log.Debug("Failed message saved as: {}", filePath);
    }
    
    public void EnqueueForwardingRequest(ForwardingRequest request)
    {
        _actionBlock.Post(request);
    }
}