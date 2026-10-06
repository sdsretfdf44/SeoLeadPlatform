using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;
using SeoLeadPlatform.Infrastructure.Http;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public class TechnicalSeoAnalyzer : ISeoAnalyzer
{
    private readonly SafeHttpClient _http;

    public TechnicalSeoAnalyzer(SafeHttpClient http) => _http = http;

    public async Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken ct)
    {
        result.HasHttps = page.BaseUri.Scheme == Uri.UriSchemeHttps;
        result.HasCanonicalTag = !string.IsNullOrWhiteSpace(page.Canonical);
        result.HasStructuredData = page.HasStructuredData;
        result.HttpStatus = page.HttpStatus;
        result.ResponseTimeMs = page.ResponseTimeMs;
        result.PageSizeBytes = page.PageSizeBytes;

        var origin = $"{page.BaseUri.Scheme}://{page.BaseUri.Host}";
        result.HasRobotsTxt = await _http.HeadOrGetSucceedsAsync($"{origin}/robots.txt", ct);
        result.HasSitemap = await _http.HeadOrGetSucceedsAsync($"{origin}/sitemap.xml", ct);

        if (!result.HasHttps)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.TechnicalSeo,
                Severity = SeoIssueSeverity.Critical,
                Title = "HTTPS not enforced",
                Description = "The website is not served over HTTPS or does not redirect to HTTPS.",
                Recommendation = "Install a valid TLS certificate and force HTTPS site-wide.",
                Evidence = $"Scheme: {page.BaseUri.Scheme}",
                EstimatedImpact = "Trust signal loss, ranking penalty, security warning in browsers."
            });
        }

        if (!result.HasCanonicalTag)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.TechnicalSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "Missing canonical tag",
                Description = "No canonical URL declared on the analysed page.",
                Recommendation = "Add <link rel=\"canonical\" href=\"...\"> to help search engines pick the preferred URL.",
                EstimatedImpact = "Risk of duplicate content and diluted ranking signals."
            });
        }

        if (!result.HasRobotsTxt)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.Indexability,
                Severity = SeoIssueSeverity.Medium,
                Title = "robots.txt not found",
                Description = "A robots.txt file was not detected at the site root.",
                Recommendation = "Publish a robots.txt file so crawlers can discover the sitemap and rules.",
                EstimatedImpact = "Suboptimal crawl guidance, potential wasted crawl budget."
            });
        }

        if (!result.HasSitemap)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.Indexability,
                Severity = SeoIssueSeverity.Medium,
                Title = "XML sitemap not found",
                Description = "A sitemap.xml was not detected at the site root.",
                Recommendation = "Generate a sitemap.xml and reference it from robots.txt.",
                EstimatedImpact = "Slower indexation of new and updated pages."
            });
        }

        if (!result.HasStructuredData)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.StructuredData,
                Severity = SeoIssueSeverity.Medium,
                Title = "No JSON-LD structured data",
                Description = "No JSON-LD structured data detected on the page.",
                Recommendation = "Add Organization/LocalBusiness/Service schema for improved rich results eligibility.",
                EstimatedImpact = "Missed opportunity for enhanced SERP appearance."
            });
        }
    }
}
