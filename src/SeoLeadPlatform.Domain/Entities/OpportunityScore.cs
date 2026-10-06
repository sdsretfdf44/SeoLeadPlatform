using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class OpportunityScore : Entity
{
    public Guid SeoAuditId { get; set; }
    public Guid ProspectCompanyId { get; set; }

    public int Score { get; set; }
    public LeadPriority Priority { get; set; }

    public int SeoNeedScore { get; set; }
    public int BusinessPotentialScore { get; set; }
    public int ServiceFitScore { get; set; }
    public int CommercialPotentialScore { get; set; }
    public int WebsiteOpportunityScore { get; set; }
    public int CompetitiveGapScore { get; set; }
    public int ContactabilityScore { get; set; }
    public int BuyingSignalScore { get; set; }

    public string? Reasoning { get; set; }
    public string FactorsJson { get; set; } = "[]";

    public SeoAudit? SeoAudit { get; set; }
}
