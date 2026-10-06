using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;
using SeoLeadPlatform.Infrastructure.Persistence;

namespace SeoLeadPlatform.Infrastructure.BackgroundJobs;

public class SeoAuditWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ISeoAuditQueue _queue;
    private readonly ILogger<SeoAuditWorker> _logger;

    public SeoAuditWorker(IServiceScopeFactory scopes, ISeoAuditQueue queue, ILogger<SeoAuditWorker> logger)
    {
        _scopes = scopes;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SeoAuditWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid auditId;
            try
            {
                auditId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            using var scope = _scopes.CreateScope();
            try
            {
                await ProcessAsync(scope.ServiceProvider, auditId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit {AuditId} failed with unexpected error", auditId);
                await MarkFailedAsync(scope.ServiceProvider, auditId, ex.Message, stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(IServiceProvider provider, Guid auditId, CancellationToken ct)
    {
        var db = provider.GetRequiredService<ApplicationDbContext>();
        var provider1 = provider.GetRequiredService<ISeoAuditProvider>();
        var scoring = provider.GetRequiredService<IOpportunityScoringService>();

        var audit = await db.SeoAudits
            .Include(a => a.Issues)
            .Include(a => a.ProspectCompany)
            .FirstOrDefaultAsync(a => a.Id == auditId, ct);
        if (audit is null)
        {
            _logger.LogWarning("Audit {AuditId} not found", auditId);
            return;
        }

        audit.MarkRunning();
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("SeoAuditStarted {AuditId} for {Url}", auditId, audit.WebsiteUrl);

        var result = await provider1.AnalyzeAsync(audit.WebsiteUrl, ct);

        if (!result.Success)
        {
            audit.MarkFailed(result.FailureReason ?? "SEO audit failed.");
            await db.SaveChangesAsync(ct);
            _logger.LogWarning("SeoAuditFailed {AuditId}: {Reason}", auditId, audit.FailureReason);
            return;
        }

        audit.HasHttps = result.HasHttps;
        audit.HasRobotsTxt = result.HasRobotsTxt;
        audit.HasSitemap = result.HasSitemap;
        audit.HasCanonicalTag = result.HasCanonicalTag;
        audit.HasStructuredData = result.HasStructuredData;
        audit.HasMetaDescription = result.HasMetaDescription;
        audit.HasTitle = result.HasTitle;
        audit.HasH1 = result.HasH1;
        audit.IsMobileFriendlyViewport = result.IsMobileFriendlyViewport;
        audit.WordCount = result.WordCount;
        audit.InternalLinkCount = result.InternalLinkCount;
        audit.ImageCount = result.ImageCount;
        audit.ImagesMissingAlt = result.ImagesMissingAlt;
        audit.HttpStatus = result.HttpStatus;
        audit.ResponseTimeMs = result.ResponseTimeMs;
        audit.PageSizeBytes = result.PageSizeBytes;

        audit.OverallScore = result.OverallScore;
        audit.TechnicalScore = result.TechnicalScore;
        audit.OnPageScore = result.OnPageScore;
        audit.ContentScore = result.ContentScore;
        audit.PerformanceScore = result.PerformanceScore;
        audit.LocalSeoScore = result.LocalSeoScore;
        audit.AnalysisSummary = result.AnalysisSummary;

        db.SeoIssues.RemoveRange(audit.Issues);
        foreach (var issue in result.Issues)
        {
            audit.Issues.Add(new SeoIssue
            {
                SeoAuditId = audit.Id,
                Category = issue.Category,
                Severity = issue.Severity,
                Title = issue.Title,
                Description = issue.Description,
                Recommendation = issue.Recommendation,
                Evidence = issue.Evidence,
                EstimatedImpact = issue.EstimatedImpact
            });
        }

        var opportunity = scoring.Score(audit.ProspectCompany!, audit);
        var existing = await db.OpportunityScores.FirstOrDefaultAsync(o => o.SeoAuditId == audit.Id, ct);
        if (existing != null) db.OpportunityScores.Remove(existing);

        db.OpportunityScores.Add(new OpportunityScore
        {
            SeoAuditId = audit.Id,
            ProspectCompanyId = audit.ProspectCompanyId,
            Score = opportunity.Score,
            Priority = opportunity.Priority,
            SeoNeedScore = opportunity.SeoNeedScore,
            BusinessPotentialScore = opportunity.BusinessPotentialScore,
            ServiceFitScore = opportunity.ServiceFitScore,
            CommercialPotentialScore = opportunity.CommercialPotentialScore,
            WebsiteOpportunityScore = opportunity.WebsiteOpportunityScore,
            CompetitiveGapScore = opportunity.CompetitiveGapScore,
            ContactabilityScore = opportunity.ContactabilityScore,
            BuyingSignalScore = opportunity.BuyingSignalScore,
            Reasoning = opportunity.Reasoning,
            FactorsJson = JsonSerializer.Serialize(opportunity.Factors)
        });

        var existingLead = await db.Leads.FirstOrDefaultAsync(l => l.ProspectCompanyId == audit.ProspectCompanyId, ct);
        if (existingLead is null)
        {
            db.Leads.Add(new Lead
            {
                AgencyId = audit.ProspectCompany!.AgencyId,
                ProspectCompanyId = audit.ProspectCompanyId,
                Status = LeadStatus.New,
                Priority = opportunity.Priority,
                OpportunityScore = opportunity.Score,
                Probability = opportunity.Score / 2
            });
        }
        else
        {
            existingLead.OpportunityScore = opportunity.Score;
            existingLead.Priority = opportunity.Priority;
            existingLead.Touch();
        }

        db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = audit.ProspectCompany!.AgencyId,
            ProspectCompanyId = audit.ProspectCompanyId,
            Type = ActivityType.AuditCompleted,
            Description = $"SEO audit completed with score {audit.OverallScore}/100 and opportunity {opportunity.Score}/100."
        });

        audit.MarkCompleted();
        await db.SaveChangesAsync(ct);
        _logger.LogInformation("SeoAuditCompleted {AuditId} score={Score} opportunity={Opportunity}", auditId, audit.OverallScore, opportunity.Score);
    }

    private async Task MarkFailedAsync(IServiceProvider provider, Guid auditId, string reason, CancellationToken ct)
    {
        try
        {
            var db = provider.GetRequiredService<ApplicationDbContext>();
            var audit = await db.SeoAudits.FirstOrDefaultAsync(a => a.Id == auditId, ct);
            if (audit != null && audit.Status != SeoAuditStatus.Failed)
            {
                audit.MarkFailed(reason);
                await db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record audit failure for {AuditId}", auditId);
        }
    }
}
