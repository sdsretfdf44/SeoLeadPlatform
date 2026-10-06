using System.Text;
using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Proposals;

public class ProposalDto
{
    public Guid Id { get; set; }
    public Guid ProspectCompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string ProblemsIdentified { get; set; } = string.Empty;
    public string RecommendedServices { get; set; } = string.Empty;
    public string Deliverables { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;
    public decimal Pricing { get; set; }
    public decimal? MonthlyRetainer { get; set; }
    public string Terms { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public string? PublicToken { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProposalRequest
{
    public string Title { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string ProblemsIdentified { get; set; } = string.Empty;
    public string RecommendedServices { get; set; } = string.Empty;
    public string Deliverables { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;
    public decimal Pricing { get; set; }
    public decimal? MonthlyRetainer { get; set; }
    public string Terms { get; set; } = string.Empty;
}

public class ProposalService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ProposalService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<ProposalDto>> GenerateForProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ProposalDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.Issues)
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.OpportunityScore)
            .Include(p => p.Lead)
            .FirstOrDefaultAsync(p => p.Id == prospectId && p.AgencyId == _current.AgencyId, ct);
        if (prospect is null) return Result.Failure<ProposalDto>("Prospect not found.", "prospect.not_found");

        var audit = prospect.Audits.FirstOrDefault();
        if (audit is null || audit.Status != SeoAuditStatus.Completed)
            return Result.Failure<ProposalDto>("Run a completed SEO audit before generating a proposal.", "proposal.audit_required");

        var services = await _db.ServiceOfferings
            .Where(s => s.AgencyId == _current.AgencyId && s.IsActive)
            .ToListAsync(ct);

        var topIssues = audit.Issues
            .OrderByDescending(i => (int)i.Severity)
            .Take(6)
            .ToList();

        var recommendedNames = RecommendServiceNames(audit, prospect, services);
        var pricing = EstimatePricing(services, recommendedNames);
        var retainer = EstimateRetainer(services, recommendedNames);

        var proposal = new Proposal
        {
            AgencyId = _current.AgencyId.Value,
            ProspectCompanyId = prospect.Id,
            LeadId = prospect.Lead?.Id,
            Title = $"SEO growth plan for {prospect.CompanyName}",
            ExecutiveSummary = BuildExecutiveSummary(prospect, audit),
            ProblemsIdentified = BuildProblems(topIssues),
            RecommendedServices = string.Join("\n", recommendedNames.Select(s => $"- {s}")),
            Deliverables = BuildDeliverables(recommendedNames),
            Timeline = "Weeks 1-2: technical audit + fixes. Weeks 3-6: on-page + content. Ongoing: reporting and iteration.",
            Pricing = pricing,
            MonthlyRetainer = retainer,
            Terms = "50% engagement fee to start; balance on milestone completion. Monthly retainer billed at the start of each month. 30-day cancellation policy.",
            Status = ProposalStatus.Draft
        };

        _db.Proposals.Add(proposal);
        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = proposal.AgencyId,
            ProspectCompanyId = prospect.Id,
            LeadId = prospect.Lead?.Id,
            UserId = _current.UserId,
            Type = ActivityType.ProposalCreated,
            Description = $"Proposal drafted for {prospect.CompanyName}"
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(proposal, prospect.CompanyName));
    }

    public async Task<Result<List<ProposalDto>>> ListForProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<List<ProposalDto>>("Not authenticated.", "auth.required");

        var proposals = await _db.Proposals
            .Where(x => x.ProspectCompanyId == prospectId && x.AgencyId == _current.AgencyId)
            .Include(x => x.ProspectCompany)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        var name = proposals.FirstOrDefault()?.ProspectCompany?.CompanyName ?? string.Empty;
        return Result.Success(proposals.Select(p => Map(p, p.ProspectCompany?.CompanyName ?? name)).ToList());
    }

    public async Task<Result<ProposalDto>> UpdateAsync(Guid id, UpdateProposalRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ProposalDto>("Not authenticated.", "auth.required");

        var proposal = await _db.Proposals
            .Include(x => x.ProspectCompany)
            .FirstOrDefaultAsync(x => x.Id == id && x.AgencyId == _current.AgencyId, ct);
        if (proposal is null) return Result.Failure<ProposalDto>("Proposal not found.", "proposal.not_found");
        if (proposal.Status != ProposalStatus.Draft) return Result.Failure<ProposalDto>("Only drafts can be edited.", "proposal.locked");

        proposal.Title = request.Title;
        proposal.ExecutiveSummary = request.ExecutiveSummary;
        proposal.ProblemsIdentified = request.ProblemsIdentified;
        proposal.RecommendedServices = request.RecommendedServices;
        proposal.Deliverables = request.Deliverables;
        proposal.Timeline = request.Timeline;
        proposal.Pricing = request.Pricing;
        proposal.MonthlyRetainer = request.MonthlyRetainer;
        proposal.Terms = request.Terms;
        proposal.Touch();

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(proposal, proposal.ProspectCompany?.CompanyName ?? string.Empty));
    }

    public async Task<Result<ProposalDto>> MarkSentAsync(Guid id, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ProposalDto>("Not authenticated.", "auth.required");

        var proposal = await _db.Proposals
            .Include(x => x.ProspectCompany)
            .FirstOrDefaultAsync(x => x.Id == id && x.AgencyId == _current.AgencyId, ct);
        if (proposal is null) return Result.Failure<ProposalDto>("Proposal not found.", "proposal.not_found");

        proposal.Status = ProposalStatus.Sent;
        proposal.SentAt = DateTime.UtcNow;
        if (string.IsNullOrEmpty(proposal.PublicToken))
        {
            proposal.PublicToken = GenerateToken();
        }
        proposal.Touch();

        if (proposal.LeadId.HasValue)
        {
            var lead = await _db.Leads.FirstOrDefaultAsync(l => l.Id == proposal.LeadId.Value, ct);
            if (lead != null)
            {
                lead.Status = LeadStatus.ProposalSent;
                lead.Touch();
            }
        }

        _db.ActivityLogs.Add(new ActivityLog
        {
            AgencyId = proposal.AgencyId,
            ProspectCompanyId = proposal.ProspectCompanyId,
            LeadId = proposal.LeadId,
            UserId = _current.UserId,
            Type = ActivityType.ProposalSent,
            Description = $"Proposal '{proposal.Title}' marked as sent"
        });

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(proposal, proposal.ProspectCompany?.CompanyName ?? string.Empty));
    }

    public async Task<Result<ProposalDto>> GetPublicAsync(string token, CancellationToken ct)
    {
        var proposal = await _db.Proposals
            .Include(x => x.ProspectCompany)
            .FirstOrDefaultAsync(x => x.PublicToken == token, ct);
        if (proposal is null) return Result.Failure<ProposalDto>("Proposal not available.", "proposal.not_found");
        return Result.Success(Map(proposal, proposal.ProspectCompany?.CompanyName ?? string.Empty));
    }

    private static ProposalDto Map(Proposal p, string company) => new()
    {
        Id = p.Id,
        ProspectCompanyId = p.ProspectCompanyId,
        CompanyName = company,
        Title = p.Title,
        ExecutiveSummary = p.ExecutiveSummary,
        ProblemsIdentified = p.ProblemsIdentified,
        RecommendedServices = p.RecommendedServices,
        Deliverables = p.Deliverables,
        Timeline = p.Timeline,
        Pricing = p.Pricing,
        MonthlyRetainer = p.MonthlyRetainer,
        Terms = p.Terms,
        Status = p.Status,
        PublicToken = p.PublicToken,
        SentAt = p.SentAt,
        CreatedAt = p.CreatedAt
    };

    private static List<string> RecommendServiceNames(SeoAudit audit, ProspectCompany prospect, List<ServiceOffering> offered)
    {
        var recommended = new List<string>();
        if (audit.TechnicalScore < 75) recommended.Add("Technical SEO");
        if (audit.OnPageScore < 75) recommended.Add("On-page SEO");
        if (audit.ContentScore < 70) recommended.Add("Content marketing");
        if (audit.LocalSeoScore < 75 && !string.IsNullOrWhiteSpace(prospect.City)) recommended.Add("Local SEO");
        if (audit.PerformanceScore < 70) recommended.Add("Performance & Core Web Vitals");

        if (offered.Count > 0)
        {
            var offeredNames = offered.Select(o => o.Name).ToList();
            var enriched = recommended
                .Select(r => offeredNames.FirstOrDefault(o => o.Contains(r.Split(' ')[0], StringComparison.OrdinalIgnoreCase)) ?? r)
                .Distinct()
                .ToList();
            return enriched;
        }
        return recommended.Count > 0 ? recommended : new List<string> { "SEO advisory" };
    }

    private static decimal EstimatePricing(List<ServiceOffering> services, List<string> recommendedNames)
    {
        if (services.Count == 0) return 5000m;
        var matched = services.Where(s => recommendedNames.Any(r => s.Name.Contains(r.Split(' ')[0], StringComparison.OrdinalIgnoreCase))).ToList();
        if (matched.Count == 0) matched = services.Take(1).ToList();
        var total = matched.Sum(s => s.TypicalProjectValue ?? 0m);
        return total == 0m ? 5000m : total;
    }

    private static decimal? EstimateRetainer(List<ServiceOffering> services, List<string> recommendedNames)
    {
        if (services.Count == 0) return null;
        var matched = services.Where(s => recommendedNames.Any(r => s.Name.Contains(r.Split(' ')[0], StringComparison.OrdinalIgnoreCase))).ToList();
        var retainer = matched.Sum(s => s.TypicalMonthlyRetainer ?? 0m);
        return retainer == 0m ? null : retainer;
    }

    private static string BuildExecutiveSummary(ProspectCompany prospect, SeoAudit audit)
    {
        return $"Our analysis of {prospect.CompanyName}'s online presence shows an overall SEO score of {audit.OverallScore}/100 " +
               $"with meaningful opportunities in technical, on-page, and content areas. " +
               $"This proposal outlines the plan to close those gaps and turn organic search into a reliable growth channel.";
    }

    private static string BuildProblems(List<SeoIssue> issues)
    {
        if (issues.Count == 0) return "No major issues identified in the initial audit.";
        var sb = new StringBuilder();
        foreach (var issue in issues)
        {
            sb.AppendLine($"- [{issue.Severity}] {issue.Title}: {issue.Description}");
        }
        return sb.ToString().Trim();
    }

    private static string BuildDeliverables(List<string> services)
    {
        var sb = new StringBuilder();
        foreach (var svc in services)
        {
            sb.AppendLine($"- {svc}: audit, prioritised backlog, implementation, and monthly reporting");
        }
        return sb.ToString().Trim();
    }

    private static string GenerateToken()
    {
        var bytes = new byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
