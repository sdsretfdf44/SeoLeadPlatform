using SeoLeadPlatform.Application.SeoAudits;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public interface ISeoAnalyzer
{
    Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken cancellationToken);
}
