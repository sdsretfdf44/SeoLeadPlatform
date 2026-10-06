using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Entities;

namespace SeoLeadPlatform.Application.Agency;

public class AgencyService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public AgencyService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<AgencyDto>> GetCurrentAsync(CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<AgencyDto>("Not authenticated.", "auth.required");

        var agency = await _db.Agencies
            .Include(a => a.ServiceOfferings)
            .FirstOrDefaultAsync(a => a.Id == _current.AgencyId, ct);

        if (agency is null) return Result.Failure<AgencyDto>("Agency not found.", "agency.not_found");

        return Result.Success(Map(agency));
    }

    public async Task<Result<AgencyDto>> UpdateAsync(UpdateAgencyRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<AgencyDto>("Not authenticated.", "auth.required");

        var agency = await _db.Agencies
            .Include(a => a.ServiceOfferings)
            .FirstOrDefaultAsync(a => a.Id == _current.AgencyId, ct);
        if (agency is null) return Result.Failure<AgencyDto>("Agency not found.", "agency.not_found");

        agency.Name = request.Name;
        agency.WebsiteUrl = request.WebsiteUrl;
        agency.Description = request.Description;
        agency.TargetIndustries = request.TargetIndustries;
        agency.TargetLocations = request.TargetLocations;
        agency.TargetServices = request.TargetServices;
        agency.MinimumProjectValue = request.MinimumProjectValue;
        agency.PreferredProjectTypes = request.PreferredProjectTypes;
        agency.Touch();

        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(agency));
    }

    public async Task<Result<ServiceOfferingDto>> AddServiceOfferingAsync(CreateServiceOfferingRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ServiceOfferingDto>("Not authenticated.", "auth.required");

        var offering = new ServiceOffering
        {
            AgencyId = _current.AgencyId.Value,
            Name = request.Name,
            Description = request.Description,
            TypicalProjectValue = request.TypicalProjectValue,
            TypicalMonthlyRetainer = request.TypicalMonthlyRetainer,
            IsActive = true
        };

        _db.ServiceOfferings.Add(offering);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new ServiceOfferingDto
        {
            Id = offering.Id,
            Name = offering.Name,
            Description = offering.Description,
            TypicalProjectValue = offering.TypicalProjectValue,
            TypicalMonthlyRetainer = offering.TypicalMonthlyRetainer,
            IsActive = offering.IsActive
        });
    }

    private static AgencyDto Map(Domain.Entities.Agency agency) => new()
    {
        Id = agency.Id,
        Name = agency.Name,
        WebsiteUrl = agency.WebsiteUrl,
        Description = agency.Description,
        TargetIndustries = agency.TargetIndustries,
        TargetLocations = agency.TargetLocations,
        TargetServices = agency.TargetServices,
        MinimumProjectValue = agency.MinimumProjectValue,
        PreferredProjectTypes = agency.PreferredProjectTypes,
        ServiceOfferings = agency.ServiceOfferings.Select(o => new ServiceOfferingDto
        {
            Id = o.Id,
            Name = o.Name,
            Description = o.Description,
            TypicalProjectValue = o.TypicalProjectValue,
            TypicalMonthlyRetainer = o.TypicalMonthlyRetainer,
            IsActive = o.IsActive
        }).ToList()
    };
}
