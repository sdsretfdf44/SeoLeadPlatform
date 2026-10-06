namespace SeoLeadPlatform.Domain.Common;

public abstract class AgencyOwnedEntity : Entity
{
    public Guid AgencyId { get; set; }
}
