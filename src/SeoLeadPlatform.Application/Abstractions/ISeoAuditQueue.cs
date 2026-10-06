namespace SeoLeadPlatform.Application.Abstractions;

public interface ISeoAuditQueue
{
    void Enqueue(Guid seoAuditId);
    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}
