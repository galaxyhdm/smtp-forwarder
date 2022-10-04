using System.Buffers;
using MimeKit;
using NLog;

namespace SmtpForwarder.SmtpReceiverServer.Extensions;

internal static class MimeMessageLoader
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static async Task<MimeMessage> ToMimeMessageAsync(this ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken = default) {
        await using var stream = new MemoryStream();
        var position = buffer.GetPosition(0);

        while (buffer.TryGet(ref position, out var memory))
            await stream.WriteAsync(memory, cancellationToken);

        stream.Position = 0;
        return await MimeMessage.LoadAsync(stream, cancellationToken);
    }

    public static async Task<MimeMessage?> TryToMimeMessageAsync(this ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken = default) {
        try
        {
            return await ToMimeMessageAsync(buffer, cancellationToken);
        }
        catch (Exception e) {
            Log.Error(e, "Error while parsing mime-message!");
            return null;
        }
    }

    
}