using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class SeoIssue : Entity
{
    public Guid SeoAuditId { get; set; }
    public SeoIssueCategory Category { get; set; }
    public SeoIssueSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public string? EstimatedImpact { get; set; }

    public SeoAudit? SeoAudit { get; set; }
}
