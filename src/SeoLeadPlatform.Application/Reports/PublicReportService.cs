using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.SeoAudits;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Reports;

public class PublicReportDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? AgencyName { get; set; }
    public SeoAuditDetailDto Audit { get; set; } = new();
}

public class CreatePublicReportResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string PublicPath => $"/reports/{Token}";
}

public class PublicReportService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public PublicReportService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<CreatePublicReportResponse>> CreateTokenAsync(Guid auditId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<CreatePublicReportResponse>("Not authenticated.", "auth.required");

        var audit = await _db.SeoAudits
            .Include(a => a.ProspectCompany)
            .FirstOrDefaultAsync(a => a.Id == auditId && a.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (audit is null) return Result.Failure<CreatePublicReportResponse>("Audit not found.", "audit.not_found");

        var token = new PublicAuditReportToken
        {
            SeoAuditId = audit.Id,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _db.PublicAuditReportTokens.Add(token);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new CreatePublicReportResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt
        });
    }

    public async Task<Result<PublicReportDto>> GetByTokenAsync(string token, CancellationToken ct)
    {
        var record = await _db.PublicAuditReportTokens
            .Include(x => x.SeoAudit).ThenInclude(a => a!.Issues)
            .Include(x => x.SeoAudit).ThenInclude(a => a!.OpportunityScore)
            .Include(x => x.SeoAudit).ThenInclude(a => a!.ProspectCompany)
                .ThenInclude(p => p!.Agency)
            .FirstOrDefaultAsync(x => x.Token == token, ct);

        if (record is null || record.IsRevoked || record.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<PublicReportDto>("Report not available.", "report.not_available");

        var audit = record.SeoAudit!;
        return Result.Success(new PublicReportDto
        {
            CompanyName = audit.ProspectCompany!.CompanyName,
            WebsiteUrl = audit.WebsiteUrl,
            AgencyName = audit.ProspectCompany.Agency?.Name,
            Audit = SeoAuditService.MapDetail(audit)
        });
    }

    private static string GenerateToken()
    {
        var bytes = new byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
