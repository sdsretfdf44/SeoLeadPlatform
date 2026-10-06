using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.OpportunityScoring;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.SeoAudits;

public class SeoAuditService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly ISeoAuditQueue _queue;

    public SeoAuditService(IApplicationDbContext db, ICurrentUserService current, ISeoAuditQueue queue)
    {
        _db = db;
        _current = current;
        _queue = queue;
    }

    public async Task<Result<SeoAuditDetailDto>> StartAuditAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<SeoAuditDetailDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .FirstOrDefaultAsync(p => p.Id == prospectId && p.AgencyId == _current.AgencyId, ct);
        if (prospect is null) return Result.Failure<SeoAuditDetailDto>("Prospect not found.", "prospect.not_found");

        var audit = new SeoAudit
        {
            ProspectCompanyId = prospect.Id,
            WebsiteUrl = prospect.WebsiteUrl,
            Status = SeoAuditStatus.Queued
        };
        _db.SeoAudits.Add(audit);
        await _db.SaveChangesAsync(ct);

        _queue.Enqueue(audit.Id);

        return await GetAuditAsync(audit.Id, ct);
    }

    public async Task<Result<SeoAuditDetailDto>> GetLatestForProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<SeoAuditDetailDto>("Not authenticated.", "auth.required");

        var audit = await _db.SeoAudits
            .Include(a => a.Issues)
            .Include(a => a.OpportunityScore)
            .Include(a => a.ProspectCompany)
            .Where(a => a.ProspectCompanyId == prospectId && a.ProspectCompany!.AgencyId == _current.AgencyId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (audit is null) return Result.Failure<SeoAuditDetailDto>("No audit found for prospect.", "audit.none");
        return Result.Success(MapDetail(audit));
    }

    public async Task<Result<SeoAuditDetailDto>> GetAuditAsync(Guid auditId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<SeoAuditDetailDto>("Not authenticated.", "auth.required");

        var audit = await _db.SeoAudits
            .Include(a => a.Issues)
            .Include(a => a.OpportunityScore)
            .Include(a => a.ProspectCompany)
            .FirstOrDefaultAsync(a => a.Id == auditId && a.ProspectCompany!.AgencyId == _current.AgencyId, ct);

        if (audit is null) return Result.Failure<SeoAuditDetailDto>("Audit not found.", "audit.not_found");
        return Result.Success(MapDetail(audit));
    }

    public static SeoAuditDetailDto MapDetail(SeoAudit audit)
    {
        List<OpportunityFactor> factors = new();
        if (audit.OpportunityScore is not null && !string.IsNullOrWhiteSpace(audit.OpportunityScore.FactorsJson))
        {
            try
            {
                factors = JsonSerializer.Deserialize<List<OpportunityFactor>>(audit.OpportunityScore.FactorsJson) ?? new();
            }
            catch (JsonException) { }
        }

        return new SeoAuditDetailDto
        {
            Id = audit.Id,
            ProspectCompanyId = audit.ProspectCompanyId,
            WebsiteUrl = audit.WebsiteUrl,
            Status = audit.Status,
            OverallScore = audit.OverallScore,
            TechnicalScore = audit.TechnicalScore,
            OnPageScore = audit.OnPageScore,
            ContentScore = audit.ContentScore,
            PerformanceScore = audit.PerformanceScore,
            LocalSeoScore = audit.LocalSeoScore,
            StartedAt = audit.StartedAt,
            CompletedAt = audit.CompletedAt,
            FailureReason = audit.FailureReason,
            HasHttps = audit.HasHttps,
            HasRobotsTxt = audit.HasRobotsTxt,
            HasSitemap = audit.HasSitemap,
            HasCanonicalTag = audit.HasCanonicalTag,
            HasStructuredData = audit.HasStructuredData,
            HasMetaDescription = audit.HasMetaDescription,
            HasTitle = audit.HasTitle,
            HasH1 = audit.HasH1,
            IsMobileFriendlyViewport = audit.IsMobileFriendlyViewport,
            WordCount = audit.WordCount,
            InternalLinkCount = audit.InternalLinkCount,
            ImageCount = audit.ImageCount,
            ImagesMissingAlt = audit.ImagesMissingAlt,
            HttpStatus = audit.HttpStatus,
            ResponseTimeMs = audit.ResponseTimeMs,
            PageSizeBytes = audit.PageSizeBytes,
            AnalysisSummary = audit.AnalysisSummary,
            Issues = audit.Issues
                .OrderByDescending(i => (int)i.Severity)
                .Select(i => new SeoIssueDto
                {
                    Id = i.Id,
                    Category = i.Category,
                    Severity = i.Severity,
                    Title = i.Title,
                    Description = i.Description,
                    Recommendation = i.Recommendation,
                    Evidence = i.Evidence,
                    EstimatedImpact = i.EstimatedImpact
                }).ToList(),
            OpportunityScore = audit.OpportunityScore is null ? null : new OpportunityScoreDto
            {
                Score = audit.OpportunityScore.Score,
                Priority = audit.OpportunityScore.Priority,
                Reasoning = audit.OpportunityScore.Reasoning ?? string.Empty,
                Factors = factors
            }
        };
    }
}
