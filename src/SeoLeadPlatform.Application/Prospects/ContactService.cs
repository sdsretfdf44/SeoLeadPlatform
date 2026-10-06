using Microsoft.EntityFrameworkCore;
using SeoLeadPlatform.Application.Abstractions;
using SeoLeadPlatform.Application.Common;
using SeoLeadPlatform.Application.Dtos;
using SeoLeadPlatform.Domain.Entities;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Application.Prospects;

public class UpsertContactRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsDecisionMaker { get; set; }
}

public class ContactService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _current;

    public ContactService(IApplicationDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<ContactDto>> AddAsync(Guid prospectId, UpsertContactRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ContactDto>("Not authenticated.", "auth.required");

        var prospect = await _db.Prospects
            .FirstOrDefaultAsync(p => p.Id == prospectId && p.AgencyId == _current.AgencyId, ct);
        if (prospect is null) return Result.Failure<ContactDto>("Prospect not found.", "prospect.not_found");

        var contact = new Contact
        {
            ProspectCompanyId = prospect.Id,
            FullName = request.FullName.Trim(),
            JobTitle = request.JobTitle?.Trim(),
            Email = request.Email?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            LinkedInUrl = request.LinkedInUrl?.Trim(),
            IsDecisionMaker = request.IsDecisionMaker,
            Source = ProspectSource.Manual
        };

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(contact));
    }

    public async Task<Result<ContactDto>> UpdateAsync(Guid contactId, UpsertContactRequest request, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure<ContactDto>("Not authenticated.", "auth.required");

        var contact = await _db.Contacts
            .Include(c => c.ProspectCompany)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (contact is null) return Result.Failure<ContactDto>("Contact not found.", "contact.not_found");

        contact.FullName = request.FullName.Trim();
        contact.JobTitle = request.JobTitle?.Trim();
        contact.Email = request.Email?.Trim();
        contact.PhoneNumber = request.PhoneNumber?.Trim();
        contact.LinkedInUrl = request.LinkedInUrl?.Trim();
        contact.IsDecisionMaker = request.IsDecisionMaker;
        contact.Touch();
        await _db.SaveChangesAsync(ct);
        return Result.Success(Map(contact));
    }

    public async Task<Result> DeleteAsync(Guid contactId, CancellationToken ct)
    {
        if (_current.AgencyId is null) return Result.Failure("Not authenticated.", "auth.required");

        var contact = await _db.Contacts
            .Include(c => c.ProspectCompany)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.ProspectCompany!.AgencyId == _current.AgencyId, ct);
        if (contact is null) return Result.Failure("Contact not found.", "contact.not_found");

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static ContactDto Map(Contact c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        JobTitle = c.JobTitle,
        Email = c.Email,
        PhoneNumber = c.PhoneNumber,
        LinkedInUrl = c.LinkedInUrl,
        IsDecisionMaker = c.IsDecisionMaker
    };
}
