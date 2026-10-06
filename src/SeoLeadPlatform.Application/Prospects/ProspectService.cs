using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Prospects;

public class ProspectService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ProspectService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<PagedResult<ProspectListItemDto>>> SearchAsync(ProspectSearchQuery query, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<PagedResult<ProspectListItemDto>>("Not authenticated.", "auth.required");

        var q = _db.Prospects
            .Where(p => p.AgencyId == _current.AgencyId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLowerInvariant();
            q = q.Where(p =>
                p.CompanyName.ToLower().Contains(s) ||
                p.WebsiteUrl.ToLower().Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(query.Industry))
            q = q.Where(p => p.Industry == query.Industry);
        if (!string.IsNullOrWhiteSpace(query.Country))
            q = q.Where(p => p.Country == query.Country);
        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(p => p.City == query.City);
        if (query.LeadStatus is not null)
            q = q.Where(p => p.Lead != null && p.Lead.Status == query.LeadStatus);
        if (query.Priority is not null)
            q = q.Where(p => p.Lead != null && p.Lead.Priority == query.Priority);
        if (query.MinOpportunityScore is not null)
            q = q.Where(p => p.Lead != null && p.Lead.OpportunityScore >= query.MinOpportunityScore);

        var total = await q.CountAsync(ct);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "opportunityscore" => query.SortDescending
                ? q.OrderByDescending(p => p.Lead != null ? p.Lead.OpportunityScore : 0)
                : q.OrderBy(p => p.Lead != null ? p.Lead.OpportunityScore : 0),
            "companyname" => query.SortDescending ? q.OrderByDescending(p => p.CompanyName) : q.OrderBy(p => p.CompanyName),
            _ => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt)
        };

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                LatestAuditStatus = p.Audits
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => (SeoAuditStatus?)a.Status)
                    .FirstOrDefault(),
                OverallSeoScore = p.Audits
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => (int?)a.OverallScore)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return Result.Success(new PagedResult<ProspectListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<ProspectDetailDto>> GetAsync(Guid id, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ProspectDetailDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .Include(p => p.Contacts)
            .Include(p => p.Lead)
            .Include(p => p.Audits.OrderByDescending(a => a.CreatedAt).Take(1))
                .ThenInclude(a => a.Issues)
            .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == _current.AgencyId, ct);

        if (prospect is null) return Result.Failure<ProspectDetailDto>("Prospect not found.", "prospect.not_found");

        var latest = prospect.Audits.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

        return Result.Success(new ProspectDetailDto
        {
            Id = prospect.Id,
            CompanyName = prospect.CompanyName,
            WebsiteUrl = prospect.WebsiteUrl,
            Industry = prospect.Industry,
            Country = prospect.Country,
            State = prospect.State,
            City = prospect.City,
            CompanySize = prospect.CompanySize,
            Description = prospect.Description,
            PhoneNumber = prospect.PhoneNumber,
            PrimaryEmail = prospect.PrimaryEmail,
            LinkedInUrl = prospect.LinkedInUrl,
            Source = prospect.Source,
            CreatedAt = prospect.CreatedAt,
            Contacts = prospect.Contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                FullName = c.FullName,
                JobTitle = c.JobTitle,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                LinkedInUrl = c.LinkedInUrl,
                IsDecisionMaker = c.IsDecisionMaker
            }).ToList(),
            Lead = prospect.Lead is null ? null : new LeadDto
            {
                Id = prospect.Lead.Id,
                Status = prospect.Lead.Status,
                Priority = prospect.Lead.Priority,
                OpportunityScore = prospect.Lead.OpportunityScore,
                EstimatedProjectValue = prospect.Lead.EstimatedProjectValue,
                EstimatedMonthlyRetainer = prospect.Lead.EstimatedMonthlyRetainer,
                Probability = prospect.Lead.Probability,
                NextFollowUpAt = prospect.Lead.NextFollowUpAt,
                Notes = prospect.Lead.Notes
            },
            LatestAudit = latest is null ? null : new SeoAuditSummaryDto
            {
                Id = latest.Id,
                Status = latest.Status,
                OverallScore = latest.OverallScore,
                TechnicalScore = latest.TechnicalScore,
                OnPageScore = latest.OnPageScore,
                ContentScore = latest.ContentScore,
                PerformanceScore = latest.PerformanceScore,
                LocalSeoScore = latest.LocalSeoScore,
                IssueCount = latest.Issues.Count,
                CriticalIssueCount = latest.Issues.Count(i => i.Severity == SeoIssueSeverity.Critical),
                HighIssueCount = latest.Issues.Count(i => i.Severity == SeoIssueSeverity.High),
                CompletedAt = latest.CompletedAt
            }
        });
    }

    public async Task<Result<ProspectDetailDto>> CreateAsync(CreateProspectRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ProspectDetailDto>("Not authenticated.", "auth.required");

        var normalizedUrl = NormalizeUrl(request.WebsiteUrl);
        var exists = await _db.Prospects.AnyAsync(p =>
            p.AgencyId == _current.AgencyId && p.WebsiteUrl == normalizedUrl, ct);
        if (exists)
        {
            return Result.Failure<ProspectDetailDto>("A prospect with this website already exists.", "prospect.duplicate_url");
        }

        var prospect = ProspectCompany.Create(
            _current.AgencyId.Value,
            request.CompanyName,
            normalizedUrl,
            request.Source,
            request.Industry,
            request.Country,
            request.State,
            request.City);

        prospect.CompanySize = request.CompanySize;
        prospect.Description = request.Description;
        prospect.PhoneNumber = request.PhoneNumber;
        prospect.PrimaryEmail = request.PrimaryEmail;
        prospect.LinkedInUrl = request.LinkedInUrl;

        _db.Prospects.Add(prospect);
        await _db.SaveChangesAsync(ct);

        return await GetAsync(prospect.Id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .FirstOrDefaultAsync(p => p.Id == id && p.AgencyId == _current.AgencyId, ct);
        if (prospect is null) return Result.Failure("Prospect not found.", "prospect.not_found");

        _db.Prospects.Remove(prospect);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static string NormalizeUrl(string url)
    {
        var trimmed = url.Trim();
        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "https://" + trimmed;
        }
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return $"{uri.Scheme}://{uri.Host.ToLowerInvariant()}{(uri.IsDefaultPort ? string.Empty : $":{uri.Port}")}{uri.PathAndQuery.TrimEnd('/')}";
        }
        return trimmed.ToLowerInvariant();
    }
}
