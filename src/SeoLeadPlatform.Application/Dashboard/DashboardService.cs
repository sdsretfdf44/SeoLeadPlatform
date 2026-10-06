using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Dashboard;

public class DashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public DashboardService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<DashboardSummaryDto>("Not authenticated.", "auth.required");
        var agencyId = _current.AgencyId.Value;
        var since = DateTime.UtcNow.AddDays(-30);

        var leads = _db.Leads.Where(l => l.AgencyId == agencyId).AsNoTracking();

        var funnel = new FunnelDto
        {
            Discovered = await _db.Prospects.CountAsync(p => p.AgencyId == agencyId, ct),
            Qualified = await leads.CountAsync(l => l.Status >= LeadStatus.Qualified && l.Status != LeadStatus.Disqualified && l.Status != LeadStatus.Lost, ct),
            Contacted = await leads.CountAsync(l => l.Status >= LeadStatus.Contacted && l.Status != LeadStatus.Disqualified && l.Status != LeadStatus.Lost, ct),
            Replied = await leads.CountAsync(l => l.Status >= LeadStatus.Replied && l.Status != LeadStatus.Disqualified && l.Status != LeadStatus.Lost, ct),
            Meeting = await leads.CountAsync(l => l.Status >= LeadStatus.MeetingScheduled && l.Status != LeadStatus.Disqualified && l.Status != LeadStatus.Lost, ct),
            Proposal = await leads.CountAsync(l => l.Status >= LeadStatus.ProposalSent && l.Status != LeadStatus.Disqualified && l.Status != LeadStatus.Lost, ct),
            Won = await leads.CountAsync(l => l.Status == LeadStatus.Won, ct)
        };

        var summary = new DashboardSummaryDto
        {
            NewProspects = await _db.Prospects.CountAsync(p => p.AgencyId == agencyId && p.CreatedAt >= since, ct),
            QualifiedLeads = funnel.Qualified,
            HighPriorityOpportunities = await leads.CountAsync(l => l.Priority >= LeadPriority.High, ct),
            OutreachSent = await _db.OutreachMessages
                .CountAsync(m => m.ProspectCompany!.AgencyId == agencyId && m.SentAt != null, ct),
            Replies = await _db.OutreachMessages
                .CountAsync(m => m.ProspectCompany!.AgencyId == agencyId && m.RepliedAt != null, ct),
            MeetingsScheduled = await leads.CountAsync(l => l.Status == LeadStatus.MeetingScheduled, ct),
            ProposalsSent = await _db.Proposals.CountAsync(p => p.AgencyId == agencyId && p.Status >= ProposalStatus.Sent, ct),
            WonProjects = funnel.Won,
            EstimatedPipelineValue = await leads
                .Where(l => l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost && l.Status != LeadStatus.Disqualified)
                .SumAsync(l => (decimal?)l.EstimatedProjectValue, ct) ?? 0m,
            Funnel = funnel
        };

        summary.TopOpportunities = await _db.Prospects
            .Where(p => p.AgencyId == agencyId && p.Lead != null)
            .OrderByDescending(p => p.Lead!.OpportunityScore)
            .Take(5)
            .Select(p => new ProspectListItemDto
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                WebsiteUrl = p.WebsiteUrl,
                Industry = p.Industry,
                Country = p.Country,
                City = p.City,
                OpportunityScore = p.Lead!.OpportunityScore,
                LeadStatus = p.Lead.Status,
                LeadPriority = p.Lead.Priority,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        summary.RecentProspects = await _db.Prospects
            .Where(p => p.AgencyId == agencyId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new ProspectListItemDto
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                WebsiteUrl = p.WebsiteUrl,
                Industry = p.Industry,
                Country = p.Country,
                City = p.City,
                OpportunityScore = p.Lead != null ? p.Lead.OpportunityScore : (int?)null,
                LeadStatus = p.Lead != null ? p.Lead.Status : (LeadStatus?)null,
                LeadPriority = p.Lead != null ? p.Lead.Priority : (LeadPriority?)null,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        summary.FollowUpsDue = await _db.Leads
            .Where(l => l.AgencyId == agencyId && l.NextFollowUpAt != null && l.NextFollowUpAt <= now.AddDays(2))
            .OrderBy(l => l.NextFollowUpAt)
            .Take(10)
            .Select(l => new FollowUpDto
            {
                LeadId = l.Id,
                ProspectId = l.ProspectCompanyId,
                CompanyName = l.ProspectCompany!.CompanyName,
                FollowUpAt = l.NextFollowUpAt!.Value,
                Status = l.Status.ToString()
            })
            .ToListAsync(ct);

        return Result.Success(summary);
    }
}
