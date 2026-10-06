using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class SeoAudit : Entity
{
    public Guid ProspectCompanyId { get; set; }
    public string WebsiteUrl { get; set; } = string.Empty;
    public SeoAuditStatus Status { get; set; } = SeoAuditStatus.Queued;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }

    public int OverallScore { get; set; }
    public int TechnicalScore { get; set; }
    public int OnPageScore { get; set; }
    public int ContentScore { get; set; }
    public int PerformanceScore { get; set; }
    public int LocalSeoScore { get; set; }

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

    public ProspectCompany? ProspectCompany { get; set; }
    public ICollection<SeoIssue> Issues { get; set; } = new List<SeoIssue>();
    public OpportunityScore? OpportunityScore { get; set; }

    public void MarkRunning()
    {
        Status = SeoAuditStatus.Running;
        StartedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkCompleted()
    {
        Status = SeoAuditStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkFailed(string reason)
    {
        Status = SeoAuditStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        FailureReason = reason;
        Touch();
    }
}
