using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public class PerformanceAnalyzer : ISeoAnalyzer
{
    public Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken ct)
    {
        if (result.ResponseTimeMs is > 3500)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.Performance,
                Severity = SeoIssueSeverity.High,
                Title = "Slow initial response time",
                Description = $"Server responded in {result.ResponseTimeMs}ms.",
                Recommendation = "Investigate server response time, caching, and CDN configuration.",
                Evidence = $"Time to first byte: {result.ResponseTimeMs}ms"
            });
        }
        else if (result.ResponseTimeMs is > 1500)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.Performance,
                Severity = SeoIssueSeverity.Medium,
                Title = "Server response could be faster",
                Description = $"Response time was {result.ResponseTimeMs}ms — target under 800ms.",
                Recommendation = "Add caching and consider a CDN for static assets."
            });
        }

        if (result.PageSizeBytes is > 2_500_000)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.Performance,
                Severity = SeoIssueSeverity.Medium,
                Title = "Heavy page weight",
                Description = $"Downloaded page weight is ~{result.PageSizeBytes / 1024}KB.",
                Recommendation = "Optimise images, defer non-critical scripts, enable compression."
            });
        }

        return Task.CompletedTask;
    }
}
