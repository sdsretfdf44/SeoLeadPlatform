using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Seo.Analyzers;

public class OnPageSeoAnalyzer : ISeoAnalyzer
{
    public Task AnalyzeAsync(ParsedPage page, SeoAuditResult result, CancellationToken ct)
    {
        var title = page.Title;
        result.HasTitle = !string.IsNullOrWhiteSpace(title);
        var description = page.MetaDescription;
        result.HasMetaDescription = !string.IsNullOrWhiteSpace(description);
        result.HasH1 = page.HasH1;
        result.IsMobileFriendlyViewport = page.HasViewport;
        result.ImageCount = page.Images.Count;
        result.ImagesMissingAlt = page.Images.Count(i =>
            string.IsNullOrWhiteSpace(i.GetAttributeValue("alt", null)));
        result.InternalLinkCount = page.InternalLinks.Count;

        if (!result.HasTitle)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.Critical,
                Title = "Missing <title> tag",
                Description = "The homepage does not have a title tag.",
                Recommendation = "Add a descriptive, keyword-rich title tag under 60 characters."
            });
        }
        else if (title!.Length > 65)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.Low,
                Title = "Title tag is long",
                Description = $"Title is {title.Length} characters and may be truncated in SERPs.",
                Recommendation = "Aim for 50-60 characters to avoid truncation.",
                Evidence = title
            });
        }

        if (!result.HasMetaDescription)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.High,
                Title = "Missing meta description",
                Description = "The page has no meta description.",
                Recommendation = "Write a compelling 140-160 character description that highlights the value proposition."
            });
        }

        if (!result.HasH1)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.High,
                Title = "Missing H1 heading",
                Description = "The page does not contain an H1 heading.",
                Recommendation = "Add a single H1 that clearly summarises the page topic."
            });
        }

        if (!result.IsMobileFriendlyViewport)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.High,
                Title = "Missing mobile viewport meta",
                Description = "No <meta name=\"viewport\"> tag detected — likely poor mobile rendering.",
                Recommendation = "Add <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">."
            });
        }

        if (result.ImagesMissingAlt > 0)
        {
            var severity = result.ImagesMissingAlt >= 5 ? SeoIssueSeverity.Medium : SeoIssueSeverity.Low;
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = severity,
                Title = "Images missing alt attributes",
                Description = $"{result.ImagesMissingAlt} of {result.ImageCount} images have no alt text.",
                Recommendation = "Add descriptive alt attributes to all meaningful images for accessibility and image search."
            });
        }

        if (result.InternalLinkCount < 5)
        {
            result.Issues.Add(new SeoIssueRecord
            {
                Category = SeoIssueCategory.OnPageSeo,
                Severity = SeoIssueSeverity.Medium,
                Title = "Sparse internal linking",
                Description = $"Only {result.InternalLinkCount} internal links detected.",
                Recommendation = "Expand internal linking to distribute authority to service and location pages."
            });
        }

        return Task.CompletedTask;
    }
}
