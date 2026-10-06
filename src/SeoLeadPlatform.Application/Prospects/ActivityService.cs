using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Prospects;

public class ActivityDto
{
    public Guid Id { get; set; }
    public ActivityType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ActivityService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ActivityService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<List<ActivityDto>>> ListForProspectAsync(Guid prospectId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<List<ActivityDto>>("Not authenticated.", "auth.required");

        var items = await _db.ActivityLogs
            .Where(a => a.AgencyId == _current.AgencyId && a.ProspectCompanyId == prospectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type,
                Description = a.Description,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return Result.Success(items);
    }
}
