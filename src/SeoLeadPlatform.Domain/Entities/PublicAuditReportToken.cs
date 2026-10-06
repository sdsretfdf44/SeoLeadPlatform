using SeoLeadPlatform.Domain.Common;

namespace SeoLeadPlatform.Domain.Entities;

public class PublicAuditReportToken : Entity
{
    public Guid SeoAuditId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public SeoAudit? SeoAudit { get; set; }
}
