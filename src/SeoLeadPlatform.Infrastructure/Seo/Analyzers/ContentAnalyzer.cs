using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public class ContentAnalyzer : ISeoAnalyzer
{
    public Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken ct)
    {
        result.WordCount = page.WordCount;

        if (result.WordCount < 150)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.ContentSeo,
                Severity = SeoIssueSeverity.High,
                Title = "Thin content on home page",
                Description = $"Only {result.WordCount} words of visible content detected.",
                Recommendation = "Expand the home page with clearly written value proposition, service overview, and social proof.",
                EstimatedImpact = "Poor topical signal to search engines and low conversion clarity."
            });
        }
        else if (result.WordCount < 350)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.ContentSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "Low content depth",
                Description = $"Home page has only {result.WordCount} words of copy.",
                Recommendation = "Add supporting content sections explaining services, differentiators, and outcomes."
            });
        }

        return Task.CompletedTask;
    }
}
