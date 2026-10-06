using SeoLeadPlatform.Domain.Common;
using SeoLeadPlatform.Domain.Enums;

namespace SeoLeadPlatform.Domain.Entities;

public class Contact : Entity
{
    public Guid ProspectCompanyId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsDecisionMaker { get; set; }
    public ProspectSource Source { get; set; } = ProspectSource.Manual;

    public ProspectCompany? ProspectCompany { get; set; }
}
