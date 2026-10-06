using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class Proposal : AgencyOwnedEntity
{
    public Guid ProspectCompanyId { get; set; }
    public Guid? LeadId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string ProblemsIdentified { get; set; } = string.Empty;
    public string RecommendedServices { get; set; } = string.Empty;
    public string Deliverables { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;
    public decimal Pricing { get; set; }
    public decimal? MonthlyRetainer { get; set; }
    public string Terms { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? PublicToken { get; set; }

    public ProspectCompany? ProspectCompany { get; set; }
    public Lead? Lead { get; set; }
}
