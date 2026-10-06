using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.SeoAudits;

public class SeoAuditResult
{
    public string WebsiteUrl { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }

    public bool HasHttps { get; set; }
    public bool HasRobotsTxt { get; set; }
    public bool HasSitemap { get; set; }
    public bool HasCanonicalTag { get; set; }
    public bool HasStructuredData { get; set; }
    public bool HasMetaDescription { get; set; }
    public bool HasTitle { get; set; }
    public bool HasH1 { get; set; }
    public bool IsMobileFriendlyViewport { get; set; }

    public int WordCount { get; set; }
    public int InternalLinkCount { get; set; }
    public int ImageCount { get; set; }
    public int ImagesMissingAlt { get; set; }

    public int? HttpStatus { get; set; }
    public long? ResponseTimeMs { get; set; }
    public long? PageSizeBytes { get; set; }

    public int TechnicalScore { get; set; }
    public int OnPageScore { get; set; }
    public int ContentScore { get; set; }
    public int PerformanceScore { get; set; }
    public int LocalSeoScore { get; set; }
    public int OverallScore { get; set; }

    public List<SeoIssueRecord> Issues { get; set; } = new();
    public string? AnalysisSummary { get; set; }
}

public class SeoIssueRecord
{
    public SeoIssueCategory Category { get; set; }
    public SeoIssueSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public string? EstimatedImpact { get; set; }
}

public class SeoIssueDto
{
    public Guid Id { get; set; }
    public SeoIssueCategory Category { get; set; }
    public SeoIssueSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public string? EstimatedImpact { get; set; }
}

public class SeoAuditDetailDto
{
    public Guid Id { get; set; }
    public Guid ProspectCompanyId { get; set; }
    public string WebsiteUrl { get; set; } = string.Empty;
    public SeoAuditStatus Status { get; set; }
    public int OverallScore { get; set; }
    public int TechnicalScore { get; set; }
    public int OnPageScore { get; set; }
    public int ContentScore { get; set; }
    public int PerformanceScore { get; set; }
    public int LocalSeoScore { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public bool HasHttps { get; set; }
    public bool HasRobotsTxt { get; set; }
    public bool HasSitemap { get; set; }
    public bool HasCanonicalTag { get; set; }
    public bool HasStructuredData { get; set; }
    public bool HasMetaDescription { get; set; }
    public bool HasTitle { get; set; }
    public bool HasH1 { get; set; }
    public bool IsMobileFriendlyViewport { get; set; }
    public int WordCount { get; set; }
    public int InternalLinkCount { get; set; }
    public int ImageCount { get; set; }
    public int ImagesMissingAlt { get; set; }
    public int? HttpStatus { get; set; }
    public long? ResponseTimeMs { get; set; }
    public long? PageSizeBytes { get; set; }

    public string? AnalysisSummary { get; set; }
    public List<SeoIssueDto> Issues { get; set; } = new();
    public OpportunityScoring.OpportunityScoreDto? OpportunityScore { get; set; }
}
