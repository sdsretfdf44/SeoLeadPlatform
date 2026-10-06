using SeoLeadPlatform.Application.SeoAudits;

namespace SeoLeadPlatform.Application.Abstractions;

public interface ISeoAuditProvider
{
    Task<SeoAuditResult> AnalyzeAsync(string websiteUrl, CancellationToken cancellationToken);
}
