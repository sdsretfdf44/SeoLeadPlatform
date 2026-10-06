using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class ActivityLog : AgencyOwnedEntity
{
    public Guid? LeadId { get; set; }
    public Guid? ProspectCompanyId { get; set; }
    public Guid? UserId { get; set; }
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}
