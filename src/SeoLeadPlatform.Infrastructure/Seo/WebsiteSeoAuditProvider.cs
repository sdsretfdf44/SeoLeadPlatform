using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Enums;
using SeoLeadPlatform.Infrastructure.Http;
using SeoLeadPlatform.Infrastructure.Seo.Analyzers;

namespace SeoLeadPlatform.Infrastructure.Seo;

public class WebsiteSeoAuditProvider : ISeoAuditProvider
{
    private readonly SafeHttpClient _http;
    private readonly IEnumerable<ISeoAnalyzer> _analyzers;
    private readonly ILogger<WebsiteSeoAuditProvider> _logger;

    public WebsiteSeoAuditProvider(
        SafeHttpClient http,
        IEnumerable<ISeoAnalyzer> analyzers,
        ILogger<WebsiteSeoAuditProvider> logger)
    {
        _http = http;
        _analyzers = analyzers;
        _logger = logger;
    }

    public async Task<SeoAuditResult> AnalyzeAsync(string websiteUrl, CancellationToken ct)
    {
        var result = new SeoAuditResult { WebsiteUrl = websiteUrl };
        var fetch = await _http.FetchAsync(websiteUrl, ct);

        result.HttpStatus = fetch.StatusCode is null ? null : (int)fetch.StatusCode;
        result.ResponseTimeMs = fetch.ResponseTimeMs;
        result.PageSizeBytes = fetch.BodyBytes;

        if (!fetch.Success || string.IsNullOrEmpty(fetch.Body))
        {
            result.Success = false;
            result.FailureReason = fetch.FailureReason ?? "Unknown fetch failure.";
            _logger.LogWarning("SEO fetch failed for {Url}: {Reason}", websiteUrl, result.FailureReason);
            return result;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(fetch.Body);

        var baseUri = new Uri(fetch.FinalUrl ?? websiteUrl);
        var page = new ParsedPage
        {
            Url = fetch.FinalUrl ?? websiteUrl,
            Html = fetch.Body,
            Document = doc,
            BaseUri = baseUri,
            HttpStatus = result.HttpStatus,
            ResponseTimeMs = result.ResponseTimeMs,
            PageSizeBytes = result.PageSizeBytes
        };

        foreach (var analyzer in _analyzers)
        {
            try
            {
                await analyzer.AnalyzeAsync(page, result, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analyzer {Analyzer} failed for {Url}", analyzer.GetType().Name, websiteUrl);
                result.Issues.Add(new SeoIssueRecord
                {
                    Category = SeoIssueCategory.TechnicalSeo,
                    Severity = SeoIssueSeverity.Low,
                    Title = $"{analyzer.GetType().Name} did not complete",
                    Description = ex.Message,
                    Recommendation = "Retry the audit — a partial analysis was recorded."
                });
            }
        }

        CalculateScores(result);
        result.AnalysisSummary = BuildSummary(result);
        result.Success = true;
        return result;
    }

    private static void CalculateScores(SeoAuditResult result)
    {
        result.TechnicalScore = ScoreCategory(result, new[] { SeoIssueCategory.TechnicalSeo, SeoIssueCategory.Indexability, SeoIssueCategory.StructuredData });
        result.OnPageScore = ScoreCategory(result, new[] { SeoIssueCategory.OnPageSeo });
        result.ContentScore = ScoreCategory(result, new[] { SeoIssueCategory.ContentSeo });
        result.PerformanceScore = ScoreCategory(result, new[] { SeoIssueCategory.Performance });
        result.LocalSeoScore = ScoreCategory(result, new[] { SeoIssueCategory.LocalSeo });

        result.OverallScore = (int)Math.Round(
            (result.TechnicalScore * 0.25) +
            (result.OnPageScore * 0.25) +
            (result.ContentScore * 0.2) +
            (result.PerformanceScore * 0.15) +
            (result.LocalSeoScore * 0.15));
    }

    private static int ScoreCategory(SeoAuditResult result, SeoIssueCategory[] categories)
    {
        var relevant = result.Issues.Where(i => categories.Contains(i.Category)).ToList();
        var penalty = relevant.Sum(i => i.Severity switch
        {
            SeoIssueSeverity.Critical => 25,
            SeoIssueSeverity.High => 15,
            SeoIssueSeverity.Medium => 8,
            SeoIssueSeverity.Low => 3,
            _ => 1
        });
        return Math.Clamp(100 - penalty, 0, 100);
    }

    private static string BuildSummary(SeoAuditResult result)
    {
        var counts = result.Issues.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count());
        return $"Overall SEO score {result.OverallScore}/100. " +
               $"Detected {counts.GetValueOrDefault(SeoIssueSeverity.Critical)} critical, " +
               $"{counts.GetValueOrDefault(SeoIssueSeverity.High)} high, " +
               $"{counts.GetValueOrDefault(SeoIssueSeverity.Medium)} medium and " +
               $"{counts.GetValueOrDefault(SeoIssueSeverity.Low)} low severity issues on the analysed page.";
    }
}
