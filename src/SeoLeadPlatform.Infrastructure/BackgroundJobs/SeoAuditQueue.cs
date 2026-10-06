using System.Threading.Channels;
using SeoLeadPlatform.Application.Abstractions;

namespace SeoLeadPlatform.Infrastructure.BackgroundJobs;

public class SeoAuditQueue : ISeoAuditQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public void Enqueue(Guid seoAuditId) => _channel.Writer.TryWrite(seoAuditId);

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
        => await _channel.Reader.ReadAsync(cancellationToken);
}
