using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Prospects;

public class ImportProspectRow
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ImportProspectsRequest
{
    public List<ImportProspectRow> Rows { get; set; } = new();
}

public class ImportProspectsResult
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ProspectImportService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ProspectImportService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<ImportProspectsResult>> ImportAsync(ImportProspectsRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ImportProspectsResult>("Not authenticated.", "auth.required");

        var result = new ImportProspectsResult();
        if (request.Rows.Count == 0) return Result.Success(result);
        if (request.Rows.Count > 500)
        {
            return Result.Failure<ImportProspectsResult>("Maximum 500 rows per import.", "import.too_large");
        }

        var existingUrls = new HashSet<string>(
            await _db.Prospects
                .Where(p => p.AgencyId == _current.AgencyId)
                .Select(p => p.WebsiteUrl)
                .ToListAsync(ct),
            StringComparer.OrdinalIgnoreCase);

        var seenInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            if (string.IsNullOrWhiteSpace(row.CompanyName) || string.IsNullOrWhiteSpace(row.WebsiteUrl))
            {
                result.Errors.Add($"Row {i + 1}: company name and website URL are required.");
                continue;
            }

            var normalizedUrl = NormalizeUrl(row.WebsiteUrl);
            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
            {
                result.Errors.Add($"Row {i + 1}: '{row.WebsiteUrl}' is not a valid URL.");
                continue;
            }

            if (!seenInBatch.Add(normalizedUrl))
            {
                result.Skipped++;
                continue;
            }
            if (existingUrls.Contains(normalizedUrl))
            {
                result.Skipped++;
                continue;
            }

            var prospect = ProspectCompany.Create(
                _current.AgencyId.Value,
                row.CompanyName.Trim(),
                normalizedUrl,
                ProspectSource.Import,
                row.Industry?.Trim(),
                row.Country?.Trim(),
                row.State?.Trim(),
                row.City?.Trim());
            prospect.PrimaryEmail = row.PrimaryEmail?.Trim();
            prospect.PhoneNumber = row.PhoneNumber?.Trim();

            _db.Prospects.Add(prospect);
            result.Imported++;
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success(result);
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
