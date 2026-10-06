using SeoLeadPlatform.Domain.Common;

namespace SeoLeadPlatform.Domain.Entities;

public class Agency : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? TargetIndustries { get; set; }
    public string? TargetLocations { get; set; }
    public string? TargetServices { get; set; }
    public decimal? MinimumProjectValue { get; set; }
    public string? PreferredProjectTypes { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ServiceOffering> ServiceOfferings { get; set; } = new List<ServiceOffering>();
    public ICollection<ProspectCompany> Prospects { get; set; } = new List<ProspectCompany>();
}
