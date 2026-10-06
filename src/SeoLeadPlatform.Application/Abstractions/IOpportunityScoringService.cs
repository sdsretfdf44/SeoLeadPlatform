using SeoLeadPlatform.Application.OpportunityScoring;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Abstractions;

public interface IOpportunityScoringService
{
    OpportunityScoreResult Score(ProspectCompany prospect, SeoAudit audit);
}
