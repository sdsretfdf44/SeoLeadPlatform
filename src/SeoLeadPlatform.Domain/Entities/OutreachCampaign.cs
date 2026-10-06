using SeoLeadPlatform.Domain.Common;

namespace SeoLeadPlatform.Domain.Entities;

public class OutreachCampaign : AgencyOwnedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OutreachMessage> Messages { get; set; } = new List<OutreachMessage>();
}
