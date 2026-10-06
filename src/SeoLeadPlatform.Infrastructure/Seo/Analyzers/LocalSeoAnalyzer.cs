using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public class LocalSeoAnalyzer : ISeoAnalyzer
{
    public Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken ct)
    {
        var body = page.Document.DocumentNode.InnerHtml ?? string.Empty;

        var hasPhone = System.Text.RegularExpressions.Regex.IsMatch(body, "\\+?\\d[\\d\\s\\-().]{6,}");
        var hasAddress = body.Contains("street", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("avenue", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("road", StringComparison.OrdinalIgnoreCase)
                        || System.Text.RegularExpressions.Regex.IsMatch(body, "\\b\\d{5}(-\\d{4})?\\b");
        var hasBusinessSchema = body.Contains("LocalBusiness", StringComparison.OrdinalIgnoreCase)
                              || body.Contains("Organization", StringComparison.OrdinalIgnoreCase);

        if (!hasPhone)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.LocalSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "No phone number visible on home page",
                Description = "Could not detect a phone number in the visible/marked-up content.",
                Recommendation = "Add a clickable phone number in the header or footer to support local intent and conversions."
            });
        }

        if (!hasAddress)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.LocalSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "Business address not evident",
                Description = "No clear physical address markup detected on the page.",
                Recommendation = "Publish full NAP (name, address, phone) in the footer and add PostalAddress schema."
            });
        }

        if (!hasBusinessSchema)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.LocalSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "Missing LocalBusiness / Organization schema",
                Description = "Structured data does not describe the business entity.",
                Recommendation = "Add LocalBusiness or Organization JSON-LD with name, address, phone, and geo coordinates when relevant."
            });
        }

        return Task.CompletedTask;
    }
}
