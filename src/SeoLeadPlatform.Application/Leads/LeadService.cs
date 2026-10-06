using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Leads;

public class UpdateLeadStatusRequest
{
    public LeadStatus Status { get; set; }
}

public class UpdateLeadRequest
{
    public LeadStatus Status { get; set; }
    public LeadPriority Priority { get; set; }
    public decimal? EstimatedProjectValue { get; set; }
    public decimal? EstimatedMonthlyRetainer { get; set; }
    public int Probability { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public string? Notes { get; set; }
}

public class LeadService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public LeadService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<LeadDto>> QualifyProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<LeadDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .Include(p => p.Lead)
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.OpportunityScore)
            .FirstOrDefaultAsync(p => p.Id == prospectId && p.AgencyId == _current.AgencyId, ct);

        if (prospect is null) return Result.Failure<LeadDto>("Prospect not found.", "prospect.not_found");

        var lead = prospect.Lead;
        var latestScore = prospect.Audits.FirstOrDefault()?.OpportunityScore;

        if (lead is null)
        {
            lead = new Lead
            {
                AgencyId = prospect.AgencyId,
                ProspectCompanyId = prospect.Id,
                Status = LeadStatus.Qualified,
                Priority = latestScore?.Priority ?? LeadPriority.Medium,
                OpportunityScore = latestScore?.Score ?? 0,
                Probability = MapProbability(latestScore?.Priority ?? LeadPriority.Medium)
            };
            _db.Leads.Add(lead);
        }
        else
        {
            lead.Status = LeadStatus.Qualified;
            if (latestScore is not null)
            {
                lead.OpportunityScore = latestScore.Score;
                lead.Priority = latestScore.Priority;
            }
            lead.Touch();
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = prospect.AgencyId,
            LeadId = lead.Id,
            ProspectCompanyId = prospect.Id,
            UserId = _current.UserId,
            Type = ActivityType.LeadQualified,
            Description = $"Prospect \"{prospect.CompanyName}\" qualified."
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(lead));
    }

    public async Task<Result<LeadDto>> UpdateStatusAsync(Guid leadId, UpdateLeadStatusRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<LeadDto>("Not authenticated.", "auth.required");

        var lead = await _db.Leads
            .Include(l => l.ProspectCompany)
            .FirstOrDefaultAsync(l => l.Id == leadId && l.AgencyId == _current.AgencyId, ct);
        if (lead is null) return Result.Failure<LeadDto>("Lead not found.", "lead.not_found");

        var previous = lead.Status;
        lead.Status = request.Status;
        lead.Touch();

        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = lead.AgencyId,
            LeadId = lead.Id,
            ProspectCompanyId = lead.ProspectCompanyId,
            UserId = _current.UserId,
            Type = MapActivity(request.Status),
            Description = $"Lead status changed from {previous} to {request.Status}."
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(lead));
    }

    public async Task<Result<LeadDto>> UpdateAsync(Guid leadId, UpdateLeadRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<LeadDto>("Not authenticated.", "auth.required");

        var lead = await _db.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.AgencyId == _current.AgencyId, ct);
        if (lead is null) return Result.Failure<LeadDto>("Lead not found.", "lead.not_found");

        lead.Status = request.Status;
        lead.Priority = request.Priority;
        lead.EstimatedProjectValue = request.EstimatedProjectValue;
        lead.EstimatedMonthlyRetainer = request.EstimatedMonthlyRetainer;
        lead.Probability = Math.Clamp(request.Probability, 0, 100);
        lead.NextFollowUpAt = request.NextFollowUpAt;
        lead.Notes = request.Notes;
        lead.Touch();

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(lead));
    }

    private static int MapProbability(LeadPriority priority) => priority switch
    {
        LeadPriority.Critical => 60,
        LeadPriority.High => 40,
        LeadPriority.Medium => 20,
        _ => 10
    };

    private static ActivityType MapActivity(LeadStatus status) => status switch
    {
        LeadStatus.Contacted => ActivityType.OutreachSent,
        LeadStatus.Replied => ActivityType.ReplyReceived,
        LeadStatus.MeetingScheduled => ActivityType.MeetingScheduled,
        LeadStatus.ProposalSent => ActivityType.ProposalSent,
        LeadStatus.Won => ActivityType.LeadWon,
        LeadStatus.Lost or LeadStatus.Disqualified => ActivityType.LeadLost,
        LeadStatus.Qualified => ActivityType.LeadQualified,
        _ => ActivityType.StatusChanged
    };

    private static LeadDto Map(Lead lead) => new()
    {
        Id = lead.Id,
        Status = lead.Status,
        Priority = lead.Priority,
        OpportunityScore = lead.OpportunityScore,
        EstimatedProjectValue = lead.EstimatedProjectValue,
        EstimatedMonthlyRetainer = lead.EstimatedMonthlyRetainer,
        Probability = lead.Probability,
        NextFollowUpAt = lead.NextFollowUpAt,
        Notes = lead.Notes
    };
}
