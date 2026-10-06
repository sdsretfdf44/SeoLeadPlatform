using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class Lead : AgencyOwnedEntity
{
    public Guid ProspectCompanyId { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;
    public int OpportunityScore { get; set; }
    public decimal? EstimatedProjectValue { get; set; }
    public decimal? EstimatedMonthlyRetainer { get; set; }
    public int Probability { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Notes { get; set; }

    public ProspectCompany? ProspectCompany { get; set; }
    public ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();
}
