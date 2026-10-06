using FluentAssertions;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;
using SeoLeadPlatform.Infrastructure.Services;

namespace SeoLeadPlatform.UnitTests.OpportunityScoring;

public class OpportunityScoringServiceTests
{
    private readonly OpportunityScoringService _sut = new();

    [Fact]
    public void High_severity_issues_produce_high_priority_score()
    {
        var prospect = ProspectCompany.Create(Guid.NewGuid(), "Acme Dental", "https://acme.com", ProspectSource.Manual,
            industry: "Dental", country: "US", city: "Austin");
        prospect.PrimaryEmail = "hello@acme.com";
        prospect.PhoneNumber = "555-1234";

        var audit = new SeoAudit
        {
            OverallScore = 42,
            OnPageScore = 40,
            ContentScore = 45,
            LocalSeoScore = 35,
            TechnicalScore = 50,
            PerformanceScore = 60,
            WordCount = 800,
            InternalLinkCount = 22,
            HasHttps = true,
            HasStructuredData = true,
            HttpStatus = 200,
            Issues = new List<SeoIssue>
            {
                new() { Category = SeoIssueCategory.OnPageSeo, Severity = SeoIssueSeverity.Critical, Title = "Missing title" },
                new() { Category = SeoIssueCategory.OnPageSeo, Severity = SeoIssueSeverity.High, Title = "No meta description" },
                new() { Category = SeoIssueCategory.LocalSeo, Severity = SeoIssueSeverity.High, Title = "No LocalBusiness schema" }
            }
        };

        var result = _sut.Score(prospect, audit);

        result.Score.Should().BeGreaterThan(65);
        result.Priority.Should().BeOneOf(LeadPriority.High, LeadPriority.Critical);
        result.Factors.Should().Contain(f => f.Name == "SEO Need" && f.Score > 30);
        result.Factors.Should().OnlyContain(f => f.Score >= 0 && f.Score <= 100);
    }

    [Fact]
    public void No_issues_produces_low_priority_score()
    {
        var prospect = ProspectCompany.Create(Guid.NewGuid(), "Boring Corp", "https://boring.example", ProspectSource.Manual);
        var audit = new SeoAudit
        {
            OverallScore = 95,
            OnPageScore = 95,
            ContentScore = 95,
            LocalSeoScore = 95,
            TechnicalScore = 95,
            PerformanceScore = 95,
            Issues = new List<SeoIssue>()
        };

        var result = _sut.Score(prospect, audit);

        result.Score.Should().BeLessThan(60);
        result.Priority.Should().BeOneOf(LeadPriority.Low, LeadPriority.Medium);
    }

    [Fact]
    public void Reasoning_is_explainable_and_references_top_factors()
    {
        var prospect = ProspectCompany.Create(Guid.NewGuid(), "Test", "https://test.example", ProspectSource.Manual, industry: "SaaS");
        var audit = new SeoAudit
        {
            OverallScore = 55,
            Issues = new List<SeoIssue>
            {
                new() { Category = SeoIssueCategory.OnPageSeo, Severity = SeoIssueSeverity.High, Title = "Missing H1" }
            }
        };

        var result = _sut.Score(prospect, audit);

        result.Reasoning.Should().Contain("Opportunity score");
        result.Reasoning.Should().Contain(result.Score.ToString());
    }
}
