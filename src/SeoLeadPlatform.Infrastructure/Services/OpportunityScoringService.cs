using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.OpportunityScoring;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Infrastructure.Services;

public class OpportunityScoringService : IOpportunityScoringService
{
    public OpportunityScoreResult Score(ProspectCompany prospect, SeoAudit audit)
    {
        var seoNeed = CalculateSeoNeed(audit);
        var website = CalculateWebsiteOpportunity(audit);
        var commercial = CalculateCommercialPotential(prospect, audit);
        var serviceFit = CalculateServiceFit(prospect, audit);
        var contactability = CalculateContactability(prospect);
        var competitiveGap = CalculateCompetitiveGap(audit);
        var buyingSignal = CalculateBuyingSignal(audit);
        var business = CalculateBusinessPotential(prospect);

        var factors = new List<OpportunityFactor>
        {
            new() { Name = "SEO Need", Score = seoNeed, Weight = 0.20, Reason = ExplainSeoNeed(seoNeed, audit) },
            new() { Name = "Commercial Potential", Score = commercial, Weight = 0.20, Reason = ExplainCommercial(commercial, prospect) },
            new() { Name = "Service Fit", Score = serviceFit, Weight = 0.15, Reason = ExplainServiceFit(serviceFit, audit) },
            new() { Name = "Website Opportunity", Score = website, Weight = 0.15, Reason = ExplainWebsite(website, audit) },
            new() { Name = "Competitive Gap", Score = competitiveGap, Weight = 0.10, Reason = "Estimated gap against typical competitors in category." },
            new() { Name = "Contactability", Score = contactability, Weight = 0.10, Reason = ExplainContactability(contactability, prospect) },
            new() { Name = "Buying Signals", Score = buyingSignal, Weight = 0.10, Reason = "Signals inferred from site activity and business potential." },
            new() { Name = "Business Potential", Score = business, Weight = 0.00, Reason = "Supporting signal from company size/industry." }
        };

        var weighted = factors.Where(f => f.Weight > 0).Sum(f => f.Score * f.Weight);
        var overall = (int)Math.Round(weighted);

        var priority = overall switch
        {
            >= 80 => LeadPriority.Critical,
            >= 65 => LeadPriority.High,
            >= 45 => LeadPriority.Medium,
            _ => LeadPriority.Low
        };

        return new OpportunityScoreResult
        {
            Score = overall,
            Priority = priority,
            SeoNeedScore = seoNeed,
            BusinessPotentialScore = business,
            ServiceFitScore = serviceFit,
            CommercialPotentialScore = commercial,
            WebsiteOpportunityScore = website,
            CompetitiveGapScore = competitiveGap,
            ContactabilityScore = contactability,
            BuyingSignalScore = buyingSignal,
            Factors = factors,
            Reasoning = BuildReasoning(overall, priority, factors)
        };
    }

    private static int CalculateSeoNeed(SeoAudit audit)
    {
        var criticalCount = audit.Issues.Count(i => i.Severity == SeoIssueSeverity.Critical);
        var highCount = audit.Issues.Count(i => i.Severity == SeoIssueSeverity.High);
        var mediumCount = audit.Issues.Count(i => i.Severity == SeoIssueSeverity.Medium);

        var raw = (criticalCount * 20) + (highCount * 12) + (mediumCount * 5);
        return Math.Clamp(raw, 0, 100);
    }

    private static int CalculateWebsiteOpportunity(SeoAudit audit)
    {
        var gap = 100 - audit.OverallScore;
        if (audit.OverallScore == 0) gap = 60;
        return Math.Clamp(gap, 0, 100);
    }

    private static int CalculateCommercialPotential(ProspectCompany prospect, SeoAudit audit)
    {
        var value = 50;
        if (!string.IsNullOrWhiteSpace(prospect.Industry))
        {
            value += HighIntentIndustry(prospect.Industry) ? 25 : 0;
        }
        if (audit.WordCount > 500) value += 10;
        if (audit.HasHttps) value += 5;
        if (!string.IsNullOrWhiteSpace(prospect.CompanySize)) value += 5;
        return Math.Clamp(value, 0, 100);
    }

    private static int CalculateServiceFit(ProspectCompany prospect, SeoAudit audit)
    {
        var value = 55;
        if (audit.LocalSeoScore < 70 && !string.IsNullOrWhiteSpace(prospect.City)) value += 15;
        if (audit.OnPageScore < 70) value += 15;
        if (audit.ContentScore < 60) value += 10;
        return Math.Clamp(value, 0, 100);
    }

    private static int CalculateContactability(ProspectCompany prospect)
    {
        var value = 30;
        if (!string.IsNullOrWhiteSpace(prospect.PrimaryEmail)) value += 30;
        if (!string.IsNullOrWhiteSpace(prospect.PhoneNumber)) value += 20;
        if (!string.IsNullOrWhiteSpace(prospect.LinkedInUrl)) value += 20;
        return Math.Clamp(value, 0, 100);
    }

    private static int CalculateCompetitiveGap(SeoAudit audit)
    {
        if (audit.OverallScore == 0) return 50;
        return Math.Clamp(100 - audit.OverallScore + 10, 0, 100);
    }

    private static int CalculateBuyingSignal(SeoAudit audit)
    {
        var value = 40;
        if (audit.HasHttps) value += 10;
        if (audit.WordCount > 300) value += 10;
        if (audit.InternalLinkCount > 10) value += 10;
        if (audit.HasStructuredData) value += 10;
        if (audit.HttpStatus == 200) value += 10;
        return Math.Clamp(value, 0, 100);
    }

    private static int CalculateBusinessPotential(ProspectCompany prospect)
    {
        var value = 45;
        if (!string.IsNullOrWhiteSpace(prospect.Industry) && HighIntentIndustry(prospect.Industry)) value += 25;
        if (!string.IsNullOrWhiteSpace(prospect.CompanySize)) value += 15;
        if (!string.IsNullOrWhiteSpace(prospect.Country)) value += 5;
        return Math.Clamp(value, 0, 100);
    }

    private static bool HighIntentIndustry(string industry)
    {
        var s = industry.Trim().ToLowerInvariant();
        return s.Contains("law") || s.Contains("dental") || s.Contains("medical")
            || s.Contains("clinic") || s.Contains("saas") || s.Contains("e-commerce")
            || s.Contains("ecommerce") || s.Contains("real estate") || s.Contains("insurance")
            || s.Contains("finance") || s.Contains("plumb") || s.Contains("roof")
            || s.Contains("hvac") || s.Contains("dentist");
    }

    private static string ExplainSeoNeed(int score, SeoAudit audit)
    {
        var counts = audit.Issues.GroupBy(i => i.Severity).ToDictionary(g => g.Key, g => g.Count());
        if (score >= 80) return $"Multiple high-impact issues detected ({counts.GetValueOrDefault(SeoIssueSeverity.Critical)} critical, {counts.GetValueOrDefault(SeoIssueSeverity.High)} high).";
        if (score >= 50) return $"Meaningful SEO gaps detected ({counts.GetValueOrDefault(SeoIssueSeverity.High)} high-priority issues).";
        return "Few obvious SEO issues detected from public analysis.";
    }

    private static string ExplainCommercial(int score, ProspectCompany prospect)
    {
        if (score >= 75) return $"Industry '{prospect.Industry}' typically has strong commercial intent and healthy per-client value.";
        if (score >= 55) return "Moderate commercial potential based on available information.";
        return "Limited commercial signal from available data — verify manually.";
    }

    private static string ExplainServiceFit(int score, SeoAudit audit)
    {
        if (score >= 80) return "Weak local/on-page/content signals map directly to our core service offerings.";
        if (score >= 60) return "Some alignment between site gaps and services offered.";
        return "Weaker fit — audit does not clearly point to a core service.";
    }

    private static string ExplainWebsite(int score, SeoAudit audit)
    {
        if (score >= 70) return $"Overall SEO score is {audit.OverallScore}/100 with significant room to improve.";
        return $"Site scores {audit.OverallScore}/100 — smaller upside available.";
    }

    private static string ExplainContactability(int score, ProspectCompany prospect)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(prospect.PrimaryEmail)) parts.Add("email");
        if (!string.IsNullOrWhiteSpace(prospect.PhoneNumber)) parts.Add("phone");
        if (!string.IsNullOrWhiteSpace(prospect.LinkedInUrl)) parts.Add("LinkedIn");
        return parts.Count > 0
            ? $"Contact channels available: {string.Join(", ", parts)}."
            : "No direct contact channels stored yet — enrichment recommended.";
    }

    private static string BuildReasoning(int overall, LeadPriority priority, List<OpportunityFactor> factors)
    {
        var top = factors.Where(f => f.Weight > 0).OrderByDescending(f => f.Score * f.Weight).Take(2).ToList();
        var reasons = string.Join(" ", top.Select(t => $"{t.Name}: {t.Reason}"));
        return $"Opportunity score {overall}/100 ({priority}). {reasons}";
    }
}
