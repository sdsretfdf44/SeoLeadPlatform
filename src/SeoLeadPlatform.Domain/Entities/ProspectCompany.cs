using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class ProspectCompany : AgencyOwnedEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? CompanySize { get; set; }
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? LinkedInUrl { get; set; }
    public ProspectSource Source { get; set; } = ProspectSource.Manual;

    public Agency? Agency { get; set; }
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<SeoAudit> Audits { get; set; } = new List<SeoAudit>();
    public Lead? Lead { get; set; }

    public static ProspectCompany Create(
        Guid agencyId,
        string companyName,
        string websiteUrl,
        ProspectSource source,
        string? industry = null,
        string? country = null,
        string? state = null,
        string? city = null)
    {
        return new ProspectCompany
        {
            AgencyId = agencyId,
            CompanyName = companyName,
            WebsiteUrl = websiteUrl,
            Source = source,
            Industry = industry,
            Country = country,
            State = state,
            City = city
        };
    }
}
