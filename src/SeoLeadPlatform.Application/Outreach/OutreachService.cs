using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Outreach;

public class GenerateOutreachRequest
{
    public Guid ProspectId { get; set; }
    public Guid? ContactId { get; set; }
    public OutreachMessageType Type { get; set; } = OutreachMessageType.InitialEmail;
}

public class OutreachMessageDto
{
    public Guid Id { get; set; }
    public Guid ProspectCompanyId { get; set; }
    public Guid? ContactId { get; set; }
    public OutreachMessageType Type { get; set; }
    public OutreachMessageStatus Status { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool GeneratedByAi { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateOutreachRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class OutreachService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly ILeadIntelligenceService _intelligence;

    public OutreachService(IApplicationDbContext db, ICurrentUserService current, ILeadIntelligenceService intelligence)
    {
        _db = db;
        _current = current;
        _intelligence = intelligence;
    }

    public async Task<Result<OutreachMessageDto>> GenerateAsync(GenerateOutreachRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<OutreachMessageDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.OpportunityScore)
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.Issues)
            .FirstOrDefaultAsync(p => p.Id == request.ProspectId && p.AgencyId == _current.AgencyId, ct);
        if (prospect is null) return Result.Failure<OutreachMessageDto>("Prospect not found.", "prospect.not_found");

        var audit = prospect.Audits.FirstOrDefault();
        if (audit is null || audit.OpportunityScore is null)
            return Result.Failure<OutreachMessageDto>("Run an SEO audit before generating outreach.", "outreach.audit_required");

        var opportunity = new OpportunityScoring.OpportunityScoreResult
        {
            Score = audit.OpportunityScore.Score,
            Priority = audit.OpportunityScore.Priority,
            Reasoning = audit.OpportunityScore.Reasoning ?? string.Empty
        };
        var intelligence = await _intelligence.AnalyzeAsync(
            new LeadIntelligenceRequest(prospect, audit, opportunity), ct);

        var subject = intelligence.SuggestedOutreachSubject ??
            $"Quick observations about {prospect.CompanyName}'s SEO";
        var body = intelligence.SuggestedOutreachBody ??
            BuildFallbackBody(prospect, audit, intelligence);

        var message = new OutreachMessage
        {
            ProspectCompanyId = prospect.Id,
            ContactId = request.ContactId,
            Type = request.Type,
            Status = OutreachMessageStatus.Draft,
            Subject = subject,
            Body = body,
            GeneratedByAi = true
        };

        _db.OutreachMessages.Add(message);
        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = prospect.AgencyId,
            ProspectCompanyId = prospect.Id,
            UserId = _current.UserId,
            Type = ActivityType.OutreachGenerated,
            Description = $"Outreach draft generated for {prospect.CompanyName}"
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(message));
    }

    public async Task<Result<OutreachMessageDto>> UpdateAsync(Guid id, UpdateOutreachRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<OutreachMessageDto>("Not authenticated.", "auth.required");

        var message = await _db.OutreachMessages
            .Include(m => m.ProspectCompany)
            .FirstOrDefaultAsync(m => m.Id == id && m.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (message is null) return Result.Failure<OutreachMessageDto>("Message not found.", "outreach.not_found");
        if (message.Status != OutreachMessageStatus.Draft)
            return Result.Failure<OutreachMessageDto>("Only drafts can be edited.", "outreach.locked");

        message.Subject = request.Subject;
        message.Body = request.Body;
        message.Touch();
        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(message));
    }

    public async Task<Result<OutreachMessageDto>> ApproveAsync(Guid id, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<OutreachMessageDto>("Not authenticated.", "auth.required");

        var message = await _db.OutreachMessages
            .Include(m => m.ProspectCompany)
            .FirstOrDefaultAsync(m => m.Id == id && m.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (message is null) return Result.Failure<OutreachMessageDto>("Message not found.", "outreach.not_found");

        message.Status = OutreachMessageStatus.Approved;
        message.ApprovedAt = DateTime.UtcNow;
        message.Touch();

        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = message.ProspectCompany!.AgencyId,
            ProspectCompanyId = message.ProspectCompanyId,
            UserId = _current.UserId,
            Type = ActivityType.OutreachApproved,
            Description = $"Outreach approved for {message.ProspectCompany!.CompanyName}"
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(message));
    }

    public async Task<Result<OutreachMessageDto>> MarkSentAsync(Guid id, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<OutreachMessageDto>("Not authenticated.", "auth.required");

        var message = await _db.OutreachMessages
            .Include(m => m.ProspectCompany)
            .FirstOrDefaultAsync(m => m.Id == id && m.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (message is null) return Result.Failure<OutreachMessageDto>("Message not found.", "outreach.not_found");

        message.Status = OutreachMessageStatus.Sent;
        message.SentAt = DateTime.UtcNow;
        if (message.ApprovedAt is null) message.ApprovedAt = DateTime.UtcNow;
        message.Touch();

        var lead = await _db.Leads.FirstOrDefaultAsync(l => l.ProspectCompanyId == message.ProspectCompanyId, ct);
        if (lead is not null && lead.Status < LeadStatus.Contacted)
        {
            lead.Status = LeadStatus.Contacted;
            lead.Touch();
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = message.ProspectCompany!.AgencyId,
            ProspectCompanyId = message.ProspectCompanyId,
            LeadId = lead?.Id,
            UserId = _current.UserId,
            Type = ActivityType.OutreachSent,
            Description = $"Outreach sent to {message.ProspectCompany!.CompanyName}"
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(message));
    }

    public async Task<Result<List<OutreachMessageDto>>> ListForProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<List<OutreachMessageDto>>("Not authenticated.", "auth.required");

        var messages = await _db.OutreachMessages
            .Include(m => m.ProspectCompany)
            .Where(m => m.ProspectCompanyId == prospectId && m.ProspectCompany!.AgencyId == _current.AgencyId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return Result.Success(messages.Select(Map).ToList());
    }

    private static string BuildFallbackBody(ProspectCompany prospect, SeoAudit audit, LeadIntelligenceResult intelligence)
    {
        var problems = intelligence.KeyProblems.Count > 0
            ? string.Join("\n- ", intelligence.KeyProblems)
            : "several improvement opportunities";
        var services = intelligence.RecommendedServices.Count > 0
            ? string.Join(", ", intelligence.RecommendedServices)
            : "SEO";

        return $"Hi there,\n\n" +
               $"I recently reviewed {prospect.CompanyName}'s site ({prospect.WebsiteUrl}) and noticed:\n- {problems}\n\n" +
               $"{intelligence.WhyThisIsAGoodProspect}\n\n" +
               $"Would a 15-minute call to walk through {services} recommendations be useful this week?\n\n" +
               $"Best regards.";
    }

    private static OutreachMessageDto Map(OutreachMessage m) => new()
    {
        Id = m.Id,
        ProspectCompanyId = m.ProspectCompanyId,
        ContactId = m.ContactId,
        Type = m.Type,
        Status = m.Status,
        Subject = m.Subject,
        Body = m.Body,
        GeneratedByAi = m.GeneratedByAi,
        ApprovedAt = m.ApprovedAt,
        SentAt = m.SentAt,
        CreatedAt = m.CreatedAt
    };
}
