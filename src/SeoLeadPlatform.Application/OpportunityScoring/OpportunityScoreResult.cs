using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.OpportunityScoring;

public class OpportunityScoreResult
{
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
    public string Reasoning { get; set; } = string.Empty;
    public List<OpportunityFactor> Factors { get; set; } = new();
}

public class OpportunityFactor
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public double Weight { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class OpportunityScoreDto
{
    public int Score { get; set; }
    public LeadPriority Priority { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<OpportunityFactor> Factors { get; set; } = new();
}
