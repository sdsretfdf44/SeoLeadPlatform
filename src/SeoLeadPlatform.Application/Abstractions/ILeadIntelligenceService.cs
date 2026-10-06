using SeoLeadPlatform.Application.OpportunityScoring;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Abstractions;

public record LeadIntelligenceRequest(ProspectCompany Prospect, SeoAudit Audit, OpportunityScoreResult Score);

public record LeadIntelligenceResult(
    string WhyThisIsAGoodProspect,
    IReadOnlyList<string> KeyProblems,
    IReadOnlyList<string> RecommendedServices,
    string? SuggestedOutreachSubject,
    string? SuggestedOutreachBody);

public interface ILeadIntelligenceService
{
    Task<LeadIntelligenceResult> AnalyzeAsync(LeadIntelligenceRequest request, CancellationToken cancellationToken);
}
