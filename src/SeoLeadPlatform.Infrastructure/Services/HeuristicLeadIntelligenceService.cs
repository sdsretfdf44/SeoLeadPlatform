using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Services;

public class HeuristicLeadIntelligenceService : ILeadIntelligenceService
{
    public Task<LeadIntelligenceResult> AnalyzeAsync(LeadIntelligenceRequest request, CancellationToken ct)
    {
        var prospect = request.Prospect;
        var audit = request.Audit;
        var score = request.Score;

        var topIssues = audit.Issues
            .OrderByDescending(i => (int)i.Severity)
            .Take(3)
            .Select(i => i.Title)
            .ToList();

        var recommendedServices = new List<string>();
        if (audit.OnPageScore < 75) recommendedServices.Add("On-page SEO");
        if (audit.ContentScore < 70) recommendedServices.Add("Content SEO");
        if (audit.LocalSeoScore < 75 && !string.IsNullOrWhiteSpace(prospect.City)) recommendedServices.Add("Local SEO");
        if (audit.TechnicalScore < 70) recommendedServices.Add("Technical SEO");
        if (audit.PerformanceScore < 70) recommendedServices.Add("Performance / CWV");
        if (recommendedServices.Count == 0) recommendedServices.Add("SEO advisory");

        var why = BuildWhy(prospect.CompanyName, score.Priority, score.Score, topIssues, recommendedServices);
        var subject = topIssues.Count > 0
            ? $"3 quick SEO wins for {prospect.CompanyName}"
            : $"Sharing observations on {prospect.CompanyName}";

        var body = BuildBody(prospect.CompanyName, prospect.WebsiteUrl, topIssues, recommendedServices);

        return Task.FromResult(new LeadIntelligenceResult(
            WhyThisIsAGoodProspect: why,
            KeyProblems: topIssues,
            RecommendedServices: recommendedServices,
            SuggestedOutreachSubject: subject,
            SuggestedOutreachBody: body));
    }

    private static string BuildWhy(string company, LeadPriority priority, int score, List<string> issues, List<string> services)
    {
        var issueSummary = issues.Count > 0 ? string.Join(", ", issues) : "several improvement opportunities";
        return $"{company} scores {score}/100 opportunity ({priority}) based on the audit. Focus on {issueSummary}. " +
               $"Recommended services: {string.Join(", ", services)}.";
    }

    private static string BuildBody(string company, string website, List<string> issues, List<string> services)
    {
        var issueList = issues.Count > 0 ? string.Join("\n- ", issues) : "several actionable improvements";
        return $"Hi,\n\n" +
               $"I took a quick look at {company}'s website ({website}) and noticed a few things worth flagging:\n- {issueList}\n\n" +
               $"These map directly to {string.Join(", ", services)}, which are areas we help clients like you improve.\n\n" +
               $"Happy to share a short walkthrough of what I found. Would 15 minutes this week work?\n\n" +
               $"Thanks.";
    }
}
